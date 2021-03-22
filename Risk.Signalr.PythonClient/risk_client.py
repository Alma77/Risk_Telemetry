# requires python -m pip install signalrcore marshmallow_dataclass --user
import argparse
from _thread import interrupt_main
from abc import ABC, abstractmethod
from typing import List, Optional, Sequence, Tuple

from marshmallow_dataclass import dataclass
from signalrcore.hub_connection_builder import HubConnectionBuilder


class MessageTypes:
    """Types of messages sent or recognized by server"""
    AttackComplete = "AttackComplete"
    AttackRequest = "AttackRequest"
    DeployRequest = "DeployRequest"
    GetStatus = "GetStatus"
    JoinConfirmation = "JoinConfirmation"
    ReceiveMessage = "ReceiveMessage"
    SendMessage = "SendMessage"
    SendStatus = "SendStatus"
    Signup = "Signup"
    YourTurnToAttack = "YourTurnToAttack"
    YourTurnToDeploy = "YourTurnToDeploy"


@dataclass
class Location:
    row: int
    column: int


@dataclass
class BoardTerritory:
    location: Location
    ownerName: Optional[str]
    armies: int


Board = List[BoardTerritory]

def board_from_dicts(board: List[dict]) -> Board:
    return [BoardTerritory.Schema().load(t) for t in board]


class AbstractRiskClient(ABC):

    ## Game Logic ##
    @abstractmethod
    def choose_deploy(self, board: Board) -> Location:
        """returns the position to which the player's next army should be deployed"""
        pass

    @abstractmethod
    def choose_attack(self, board: Board) -> Tuple[Location, Location]:
        """returns the source and target of attack"""
        pass

    @abstractmethod
    def should_attack(self, board: Board) -> bool:
        """returns True if this player should attack given the current board and False otherwise"""
        pass

    ## Helper Functions for Logic ##
    def get_neighbors(self, territory: BoardTerritory, board: Board) -> Sequence[BoardTerritory]:
        """returns a list containing all territories neighboring the given territory"""
        def is_neighbor(other: BoardTerritory):
            return max(abs(territory.location.column - other.location.column),
                       abs(territory.location.row - other.location.row)) == 1
        return [t for t in board if is_neighbor(t)]

    def get_attacks(self, board: Board) -> Sequence[Tuple[BoardTerritory, BoardTerritory]]:
        """returns all possible attacks: (srouce, dest) territory pairs"""
        print("attacking...")
        return [
            (src, tgt)
            for src in self.get_mine(board)
            for tgt in self.get_neighbors(src, board)
            if src.armies > 1 and tgt.ownerName != self.name]

    def get_deployable(self, board: Board) -> Sequence[BoardTerritory]:
        """get locations that are legal for deploying an army"""
        return list(self.get_free(board)) + list(self.get_mine(board))

    def get_free(self, board: Board) -> Sequence[BoardTerritory]:
        """returns a list of all unoccupied territories"""
        return [t for t in board if t.ownerName is None]

    def get_mine(self, board: Board) -> Sequence[BoardTerritory]:
        """returns a list of player's territories"""
        return [t for t in board if t.ownerName == self.name]

    ## Handlers ##
    # argument from signalr is actually a list of arguments (in this case a single list)
    def handle_deploy(self, dict_board: List[List[dict]]) -> None:
        print("handling deploy...")
        board: Board = board_from_dicts(*dict_board)
        deploy_location = self.choose_deploy(board)
        print(f"attempting to deploy to {deploy_location}")
        self.connection.send(
            MessageTypes.DeployRequest, [Location.Schema().dump(deploy_location)])

    def handle_attack(self, dict_board: List[List[dict]]) -> None:
        print("handling attack...")
        board = board_from_dicts(*dict_board)
        if self.should_attack(board):
            attack = self.choose_attack(board)
            (source, target) = attack
            print(f"attempting to attack from {source} to {target}")
            self.connection.send(MessageTypes.AttackRequest, [Location.Schema().dump(source),
                                                              Location.Schema().dump(target)])
        else:
            print("done attacking")
            self.connection.send(MessageTypes.AttackComplete, [])

    def handle_join(self, name: List[str]) -> None:
        """saves the name assigned by the server"""
        print(f"confirmed join as {name}")
        [self.name] = name

    def handle_open(self):
        print("connection opened and handshake received ready to send messages")
        # send our name only after connected
        self.connection.send(MessageTypes.Signup, [self.name])

    def handle_close(self):
        print("Connection to server closed.")
        # exit main
        interrupt_main()

    def handle_status(self, status):
        print("Received status report.")

    def start(self, name, server, port):
        """runs the client, communicating with the server as necessary""" 
        self.name = name
        self.connection = HubConnectionBuilder().with_url(
            f"{server}:{port}/riskhub").build()
        self.connection.on(MessageTypes.ReceiveMessage, print)
        self.connection.on(MessageTypes.SendMessage, print)
        self.connection.on(MessageTypes.SendStatus, self.handle_status)
        self.connection.on(MessageTypes.JoinConfirmation, self.handle_join)
        self.connection.on(MessageTypes.YourTurnToDeploy, self.handle_deploy)
        self.connection.on(MessageTypes.YourTurnToAttack, self.handle_attack)
        self.connection.on_open(self.handle_open)
        self.connection.on_close(self.handle_close)
        self.connection.start()

        # accept commands (mostly just waiting while the server interupts)
        command = ""
        while (command.lower() != 'exit'):
            print("Please enter a command ('exit' to exit): ", end="")
            command = input()

    def start_cli(self):
        """runs the client based on command-line arguments"""
        parser = argparse.ArgumentParser('RiskClient')
        parser.add_argument('--server', default="http://localhost")
        parser.add_argument('--port', type=int, default=5000)
        parser.add_argument('--name', default="python_person")
        args = parser.parse_args()
        try:
            self.start(name=args.name, server=args.server, port=args.port)
        except:
            print("Trouble with the connection.  Do you have the right port?")
            print(f"Here is what we used: {args}")
