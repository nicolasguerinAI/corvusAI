#!/usr/bin/env python3

import sys, json


def calc_weighted_average(val_s1, val_s2, val_s3):
    w_val_s1 = float(val_s1)
    w_val_s2 = float(val_s2 * 0.9)
    w_val_s3 = float(val_s3 * 0.8)

    w_average = (w_val_s1 + w_val_s2 + w_val_s3) / 2.7
    return round(w_average, 2)


def read_file(file_path):
    saisons = []
    with open(file_path, 'r') as file:
        for line in file:
            line = line.lstrip('\ufeff').rstrip('\n')
            tmpSplit = line.split(';')
            saison = {
                'Wins': float(tmpSplit[0]),
                'Draws': float(tmpSplit[1]),
                'Losses': float(tmpSplit[2]),
                'GoalsScored': float(tmpSplit[3]),
                'GoalsTaken': float(tmpSplit[4]),
                'ShotsAverage': float(tmpSplit[5]),
                'GoalsPerShot': float(tmpSplit[6])
            }
            saisons.append(saison)
    return saisons


def create_average_stats(saisons, name):
    stats = {
        'Name': name,
        'WinsAverage': calc_weighted_average(saisons[0]['Wins'], saisons[1]['Wins'], saisons[2]['Wins']),
        'DrawsAverage': calc_weighted_average(saisons[0]['Draws'], saisons[1]['Draws'], saisons[2]['Draws']),
        'LossesAverage': calc_weighted_average(saisons[0]['Losses'], saisons[1]['Losses'], saisons[2]['Losses']),
        'GoalsScoredAverage': calc_weighted_average(saisons[0]['GoalsScored'], saisons[1]['GoalsScored'], saisons[2]['GoalsScored']),
        'GoalsTakenAverage': calc_weighted_average(saisons[0]['GoalsTaken'], saisons[1]['GoalsTaken'], saisons[2]['GoalsTaken']),
        'ShotsAverage': calc_weighted_average(saisons[0]['ShotsAverage'], saisons[1]['ShotsAverage'], saisons[2]['ShotsAverage']),
        'GoalsPerShotAverage': calc_weighted_average(saisons[0]['GoalsPerShot'], saisons[1]['GoalsPerShot'], saisons[2]['GoalsPerShot'])
    }

    return stats


def get_name(str):
    str = str.split('/')
    str = str[len(str) - 1]
    str = str.split('.')
    return str[0]


if __name__ == '__main__':
    saisons = read_file(sys.argv[1])
    name = get_name(sys.argv[1])
    stats = create_average_stats(saisons, name)
    with open(name + '.json', 'w') as fp:
        json.dump(stats, fp)