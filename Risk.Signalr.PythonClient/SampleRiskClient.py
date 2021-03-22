from typing import Tuple
import random
from risk_client import AbstractRiskClient, Board, Location


class MyRiskClient(AbstractRiskClient):

    def choose_deploy(self, board: Board) -> Location:
        """returns the position of where the next army should be deployed"""
        return random.choice(self.get_deployable(board)).location

    def should_attack(self, board: Board) -> bool:
        """returns whether this player should attack given the current board"""
        return bool(random.randint(0, 1))

    def choose_attack(self, board: Board) -> Tuple[Location, Location]:
        """returns the source and target of attack"""
        source, target = random.choice(self.get_attacks(board))
        return source.location, target.location


# only build and start the client if this file is being executed directly
if __name__ == '__main__':
    MyRiskClient().start_cli()
