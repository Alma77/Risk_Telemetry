# risk-challenge-python-client
starter code for python client for signalr risk competition

## Getting Started

### Clone the repositorysi

First, clone the [coding challenge repository](https://github.com/SnowSE/risk_coding_challenge):

```bash
git clone https://github.com/SnowSE/risk_coding_challenge
```

### Start the server, visualizer and built-in player

Before starting up a client, make sure that you have access to a running Risk Server on a known and accessible url and port: From the `Risk.Server` folder, run `dotnet run`.

By default, you can access the visualizer (and add a built-in player to the game) via a webbrowser at [`http://localhost:5000`](http://localhost:5000) where you can also start the game.

### Install the python client prerequisites
The starter code depends on Python 3.7+ with pip installed.

From a different terminal and while now from the `Risk.Signalr.PythonClient` folder of the repository, install python client dependencies via pip (`--user` is optional if you have write permissions to python installation):
```bash
python -m pip install -r requirements.txt --user
```

### Run the sample client
To run the sample python client from the `Risk.Signalr.PythonClient` folder, run the following:

```bash
python SampleRiskClient.py
```

## Modify the Client Strategy
Now you can write your own!

`SampleRiskClient.py` gives and example client that randomly deploys to a legal board position, flips a coin to decide whether to continue attacking, and randomly chooses from all legal attacks when attacking.

`SampleRiskClient2.py` is another simple client that doesn't include comments, use type hints, etc. and is slightly less terse.  It simply returns the first valid place to deploy, always attacks if possible, and returns the first valid attack when asked.

In either case your will simply implement the logic that makes the following decisions:

1. `choose_deploy`: where to deploy your next army given the current state of the game board
2. `should_attack`: whether or not you should attack at a given the current state of the game board
3. `choose_attack`: where you should attack from and what location should be the target of your next attack given the current state of the game board

