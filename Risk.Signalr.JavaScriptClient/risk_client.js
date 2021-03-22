class Location {
    constructor(row, column) {
        this.row = row;
        this.column = column;
    }
}

class Territory {
    constructor(ownerName, armies, location) {
        this.ownerName = ownerName;
        this.armies = armies;
        this.location = location;
    }
}

class AbstractRiskClient {
    // Types of messages sent or recognized by server
    MessageTypes = {
        AttackComplete: "AttackComplete",
        AttackRequest: "AttackRequest",
        DeployRequest: "DeployRequest",
        GetStatus: "GetStatus",
        JoinConfirmation: "JoinConfirmation",
        ReceiveMessage: "ReceiveMessage",
        SendMessage: "SendMessage",
        SendStatus: "SendStatus",
        Signup: "Signup",
        YourTurnToAttack: "YourTurnToAttack",
        YourTurnToDeploy: "YourTurnToDeploy"
    }

    // ## Game Logic ##
    // returns the position to which the player's next army should be deployed"""
    choose_deploy(board) {
        throw new Error("not implemented");
    }

    // returns the source and target of attack
    choose_attack(board) {
        throw new Error("not implemented");
    }

    // returns True if this player should attack given the current board and False otherwise
    should_attack(board) {
        throw new Error("not implemented");
    }

    // ## Helper Functions for Logic ##
    // returns a list containing all territories neighboring the given territory
    get_neighbors(territory, board) {
        function is_neighbor(other) {
            return Math.max(Math.abs(territory.location.column - other.location.column),
                            Math.abs(territory.location.row - other.location.row)) == 1
        }
        return board.filter(is_neighbor);
    }

    // returns all possible attacks: (source, dest) territory pairs
    get_attacks(board) {
        console.log("attacking...");
        return this.get_mine(board)
            .filter(src => src.armies > 1)                   // can only attack with > 1
            .flatMap(src => this.get_neighbors(src, board)   // get neighbors
                .filter(tgt => tgt.ownerName !== this.name)  // skip my own lands
                .map(tgt => [src, tgt]))                     // return pairs
    }

    // get locations that are legal for deploying an army
    get_deployable(board) {
        return this.get_free(board).concat(this.get_mine(board))
    }

    // returns a list of all unoccupied territories
    get_free(board) {
        return board.filter(t => t.ownerName === null);
    }

    // returns a list of player's territories
    get_mine(board) {
        return board.filter(t => t.ownerName === this.name);
    }

    // ## Handlers ##
    // # argument from signalr is actually a list of arguments (in this case a single list)
    handle_deploy(board) {
        console.log("handling deploy...");
        let deploy_location = this.choose_deploy(board);
        console.log(`attempting to deploy to ${JSON.stringify(deploy_location)}`);
        this.connection.send(this.MessageTypes.DeployRequest, deploy_location);
    }

    handle_attack(board) {
        console.log("handling attack...")
        // it looked like I was getting invited to attack even when I had no valid attacks
        if (this.should_attack(board)) {
            let attack = this.choose_attack(board);
            let [source, target] = attack;
            console.log(`attempting to attack from ${JSON.stringify(source)} to ${JSON.stringify(target)}`);
            this.connection.send(this.MessageTypes.AttackRequest, source, target);
        } else {
            console.log("done attacking");
            this.connection.send(this.MessageTypes.AttackComplete);
        }
    }

    // saves the name assigned by the server
    handle_join(name) {
        console.log(`confirmed join as ${name}`);
        this.name = name;
    }

    handle_close() {
        console.log("Connection to server closed.");
    }

    handle_status(status) {
        console.log("Received status report.");
    }

    // runs the client, communicating with the server as necessary
    start(name="javascript_person", server="http://localhost", port=5000) {
        this.name = name;
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${server}:${port}/riskhub`)
            .build();
        this.connection.on(this.MessageTypes.ReceiveMessage, console.log);
        this.connection.on(this.MessageTypes.SendMessage, console.log);
        this.connection.on(this.MessageTypes.SendStatus, status => this.handle_status(status));
        this.connection.on(this.MessageTypes.JoinConfirmation, name => this.handle_join(name));
        this.connection.on(this.MessageTypes.YourTurnToDeploy, board => this.handle_deploy(board));
        this.connection.on(this.MessageTypes.YourTurnToAttack, board => this.handle_attack(board));
        this.connection.onclose(this.handle_close);
        this.connection.start().then(() => {
            console.log("connection opened and handshake received ready to send messages");
            // send our name only after connected
            this.connection.send(this.MessageTypes.Signup, this.name);
        });
    }
}

