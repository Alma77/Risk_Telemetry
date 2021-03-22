# less terse client that also doesn't include type hints or comments

from risk_client import AbstractRiskClient

class MyRiskClient2(AbstractRiskClient):

    def choose_deploy(self, board):
        valid_territories = self.get_deployable(board)
        first_valid_territory = valid_territories[0]
        return first_valid_territory.location

    def should_attack(self, board):
        return True

    def choose_attack(self, board):
        valid_attacks = self.get_attacks(board)
        first_valid_attack = valid_attacks[0]
        my_territory, enemy_territory = first_valid_attack
        return my_territory.location, enemy_territory.location

MyRiskClient2().start_cli()
