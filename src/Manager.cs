using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Manager
{

    private int populationSize = 500;
    private int generationNumber = 0;
    private int[] layers = new int[] { 14, 7, 1 };
    private List<NeuralNetwork> nets;
    private Dictionary<string, Team> teams;
    private List<TrainingMatch> trainingMatches;

    private NeuralNetwork bestBrain = null;
    private List<NeuralNetwork> bestBrains;
    private float bestFitness = 0;

    public Manager()
    {
        teams = new Dictionary<string, Team>();

        if (!Directory.Exists("./Teams"))
            throw new Exception("Corvus: Team directory doesn't exist");

        foreach (var file in Directory.EnumerateFiles("./Teams", "*.json"))
        {
            using (var fileContents = File.OpenText(file))
            {
                var serializer = new JsonSerializer();
                var team = (Team) serializer.Deserialize(fileContents, typeof(Team));
                teams.Add(team.Name, team);
            }
        }

        if (teams.Count == 0)
            throw new Exception("Corvus: No team data");

        if (!File.Exists("./best_brain.json"))
            throw new Exception("Corvus: No brain found");


        using (var file = File.OpenText("./best_brain.json"))
        {
            var serializer = new JsonSerializer();
            var bestBrainRead = (BestBrain) serializer.Deserialize(file, typeof(BestBrain));

            if (!bestBrainRead.FullyTrained)
                throw new Exception("Corvus: Brain is not fully trained");

            bestBrain = new NeuralNetwork(layers);
            bestBrain.CopyWeights(bestBrainRead.Weights);
        }
    }

    public Manager(string matchDataPath)
    {
        teams = new Dictionary<string, Team>();

        if (!Directory.Exists("./Teams"))
            throw new Exception("Corvus: Team directory doesn't exist");

        foreach (var file in Directory.EnumerateFiles("./Teams", "*.json"))
        {
            using (var fileContents = File.OpenText(file))
            {
                var serializer = new JsonSerializer();
                var team = (Team) serializer.Deserialize(fileContents, typeof(Team));
                teams.Add(team.Name, team);
            }
        }

        if (teams.Count == 0)
            throw new Exception("Corvus: No team data");

        using (var file = File.OpenText(matchDataPath))
        {
            var serializer = new JsonSerializer();
            trainingMatches = (List<TrainingMatch>) serializer.Deserialize(file, typeof(List<TrainingMatch>));
        }

        Console.WriteLine("Number of training matches: " + trainingMatches.Count);
    }

    public void PredictMatch(string team1Name, string team2Name)
    {
        if (!teams.ContainsKey(team1Name))
            throw new Exception("Corvus: No data for " + team1Name);

        if (!teams.ContainsKey(team2Name))
            throw new Exception("Corvus: No data for " + team2Name);

        var team1 = teams[team1Name];
        var team2 = teams[team2Name];

        float[] inputs = new float[14];

        // Team1 data
        inputs[0] = team1.WinsAverage;
        inputs[1] = team1.DrawsAverage;
        inputs[2] = team1.LossesAverage;
        inputs[3] = team1.GoalsScoredAverage;
        inputs[4] = team1.GoalsTakenAverage;
        inputs[5] = team1.ShotsAverage;
        inputs[6] = team1.GoalsPerShotAverage;
        // Team2 data
        inputs[7] = team2.WinsAverage;
        inputs[8] = team2.DrawsAverage;
        inputs[9] = team2.LossesAverage;
        inputs[10] = team2.GoalsScoredAverage;
        inputs[11] = team2.GoalsTakenAverage;
        inputs[12] = team2.ShotsAverage;
        inputs[13] = team2.GoalsPerShotAverage;

        var prediction = bestBrain.FeedForward(inputs)[0];

        if (prediction > .33f)
            Console.WriteLine("Corvus: Brain predicts that " + team1Name + " will win");
        else if (prediction < -.33f)
            Console.WriteLine("Corvus: Brain predicts that " + team2Name + " will win");
        else
            Console.WriteLine("Corvus: Brain predicts that the match will be a draw");
    }

	public void Train()
    {
        while (true) {
            if (bestBrain != null) {
                if (bestFitness < bestBrain.GetFitness()) {
                    bestFitness = bestBrain.GetFitness();
                    Console.WriteLine("Corvus: Generation: " + generationNumber);
                    Console.WriteLine("Corvus: Generation best fitness: " + bestFitness);
                    //Console.WriteLine("Corvus: Generation total best brains: " + bestBrains.Count);
                }

                // Write Best Brain to file
                var bestBrainWrite = new BestBrain()
                {
                    Generation = generationNumber,
                    Fitness = bestBrain.GetFitness(),
                    FullyTrained = bestBrain.GetFitness() == trainingMatches.Count,
                    Weights = bestBrain.GetWeights()
                };

                using (StreamWriter file = File.CreateText("./best_brain.json"))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, bestBrainWrite);
                }

                if (bestBrainWrite.FullyTrained)
                {
                    Console.WriteLine("Corvus: Finished training");
                    return;
                }

                int k = 0;
                for (int i = 0; i < populationSize; i++)
                {
                    if (k == bestBrains.Count)
                        k = 0;

                    if (bestBrains.Count > 0)
                        nets[i] = new NeuralNetwork(bestBrains[k++]);
                    else
                        nets[i] = new NeuralNetwork(bestBrain);
                    nets[i].Mutate();
                    nets[i].SetFitness(0f);
                }
            }

            generationNumber++;

            TrainingLoop();
        }
    }

    void TrainingLoop()
    {
        foreach (var match in trainingMatches)
        {
            var team1 = teams[match.Team1];
            var team2 = teams[match.Team2];
            float[] inputs = new float[14];

            // Team1 data
            inputs[0] = team1.WinsAverage;
            inputs[1] = team1.DrawsAverage;
            inputs[2] = team1.LossesAverage;
            inputs[3] = team1.GoalsScoredAverage;
            inputs[4] = team1.GoalsTakenAverage;
            inputs[5] = team1.ShotsAverage;
            inputs[6] = team1.GoalsPerShotAverage;
            // Team2 data
            inputs[7] = team2.WinsAverage;
            inputs[8] = team2.DrawsAverage;
            inputs[9] = team2.LossesAverage;
            inputs[10] = team2.GoalsScoredAverage;
            inputs[11] = team2.GoalsTakenAverage;
            inputs[12] = team2.ShotsAverage;
            inputs[13] = team2.GoalsPerShotAverage;

            //Console.WriteLine("Corvus: Match: " + team1.Name + " vs " + team2.Name);
            var index = 0;
            foreach (var brain in nets)
            {
                var prediction = brain.FeedForward(inputs)[0];

                //Console.WriteLine("Brain fitness: " + brain.GetFitness());

                if (prediction >= match.Result - .33f && prediction <= match.Result + .33f)
                {
                    brain.AddFitness(1);
                    //Console.WriteLine("Corvus: Brain " + index + " correct");
                } else
                {
                    //Console.WriteLine("Corvus: Brain " + index + " wrong");
                }

                index++;
            }
        }

        // Find best brain
        foreach (var brain in nets)
        {
            if (brain.CompareTo(bestBrain) == 1)
                bestBrain = new NeuralNetwork(brain);
        }

        var foundOne = false;
        // Find all brains with same fitness as the best brain including the best brain
        foreach (var brain in nets)
        {
            if (brain.CompareTo(bestBrain) == 0) {
                if (!foundOne) {
                    foundOne = true;
                    bestBrains.Clear();
                }
                bestBrains.Add(new NeuralNetwork(brain));
            }
        }
    }

    public void InitNeuralNetworks()
    {
        nets = new List<NeuralNetwork>();
        bestBrains = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Mutate();
            nets.Add(net);
        }

        if (File.Exists("./best_brain.json"))
        {
            using (var file = File.OpenText("./best_brain.json"))
            {
                var serializer = new JsonSerializer();
                var bestBrainRead = (BestBrain) serializer.Deserialize(file, typeof(BestBrain));

                if (bestBrainRead.FullyTrained)
                    throw new Exception("Corvus: Brain is already fully trained (if you think this is wrong, please change FullyChanged to false)");

                nets[0].CopyWeights(bestBrainRead.Weights);
                nets[0].SetFitness(bestBrainRead.Fitness);
                generationNumber = bestBrainRead.Generation;
                bestBrain = new NeuralNetwork(nets[0]);
            }
        }
    }
}
