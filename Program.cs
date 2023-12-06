using System;
using System.Collections.Generic;
using System.Linq;

namespace Routing.AI
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Desired number of clusters.
            const int NO_OF_CLUSTERS = 3;
            // Maximum weight per cluster.
            const int MAX_WEIGHT = 750;
            // Noise penalty is 50km for each noise.
            const long NOISE_PENALTY = 50000;

            Console.WriteLine("Instantiate problem: Nodes={0}, clusters={1}, max weight={2}", DataModel.NodesCount, NO_OF_CLUSTERS, MAX_WEIGHT);

            // Build the clusters list
            List<Cluster> clusters = new List<Cluster>();
            // Build the noises list
            List<Node> noises = new List<Node>();

            // First cluster
            Cluster cluster = new Cluster(clusters.Count + 1, MAX_WEIGHT);
            // Dummy solution: add nodes in order until reach the max weight of the cluster.
            // The remaining nodes, if any, are kept in noises list. Noises list have a penalty.
            for (int i = 0; i < DataModel.NodesCount; i++)
            {
                if (cluster.AddNode(i, DataModel.Weights[i]))
                {
                    // Node added in cluster
                }
                else
                {
                    if (clusters.Count < NO_OF_CLUSTERS)
                    {
                        // Add the cluster's clone to the list
                        clusters.Add(cluster.Clone());
                        // Instantiate a new empty cluster
                        cluster = new Cluster(clusters.Count + 1, MAX_WEIGHT);
                        // Add the node to the new cluster
                        cluster.AddNode(i, DataModel.Weights[i]);
                    }
                    else
                    {
                        //no more clusters
                        cluster = null;
                        // Add to noises
                        noises.Add(new Node(i, DataModel.Weights[i]));
                    }
                }
            }
            // Add the last cluster
            if (cluster != null)
                clusters.Add(cluster);

            // Print the Solution
            long solutionTotal = PrintSolution(clusters, noises, NOISE_PENALTY);
            Console.WriteLine("Solution Total {0}", solutionTotal);
        }

        private static long PrintSolution(List<Cluster> clusters, List<Node> noises, long penalty)
        {
            long overallDistances = 0;
            Console.WriteLine("Solution: clusters={0}, noises={1}:", clusters.Count, noises.Count);
            Console.WriteLine();
            foreach (Cluster cluster in clusters)
            {
                Console.WriteLine("Cluster id={0} nodes {1}:", cluster.Id, cluster.Nodes.Count);
                for (int i = 0; i < cluster.Nodes.Count; i++)
                {
                    Node current_node = cluster.Nodes[i];
                    Console.Write(current_node.Index + (i < cluster.Nodes.Count - 1 ? ", " : ""));
                }
                Console.WriteLine();
                int totalWeight = cluster.TotalWeight;
                long totalDistance = cluster.TotalDistance;
                Console.WriteLine("Cluster totals: Weight = {1} kg / Distance = {0} m", cluster.TotalDistance, cluster.TotalWeight);
                Console.WriteLine();
                overallDistances += totalDistance;
            }
            long noisesDistances = (penalty * penalty) * noises.Count;
            Console.WriteLine("Overall distances: clusters {0} m / noises {1} m", overallDistances, noisesDistances);
            Console.WriteLine();

            return overallDistances + noisesDistances;
        }

        class Cluster
        {
            public int Id { get; set; }
            public int MaxWeight { get; set; }

            public List<Node> Nodes { get; set; }

            public int TotalWeight
            {
                get
                {
                    return this.Nodes.Sum(p => p.Weight);
                }
            }

            public long TotalDistance
            {
                get
                {
                    long dist = 0;
                    for (int i = 0; i < this.Nodes.Count; i++)
                        for (int j = 0; j < this.Nodes.Count; j++)
                            if (i != j)
                                dist += DataModel.DistanceMatrix[this.Nodes[i].Index, this.Nodes[j].Index];
                    return dist;
                }
            }

            public Cluster(int id, int maxWeight)
            {
                this.Id = id;
                this.MaxWeight = maxWeight;
                this.Nodes = new List<Node>();
            }

            public bool AddNode(int index, int weight)
            {
                if (this.TotalWeight + weight > this.MaxWeight)
                    return false;
                else
                    this.Nodes.Add(new Node(index, weight));
                return true;
            }

            public Cluster Clone()
            {
                Cluster clone = new Cluster(this.Id, this.MaxWeight);
                foreach (Node node in this.Nodes)
                    clone.Nodes.Add(node.Clone());
                return clone;
            }
        }

        class Node
        {
            public int Index { get; set; }
            public int Weight { get; set; }

            public Node(int index, int weight)
            {
                this.Index = index;
                this.Weight = weight;
            }

            public Node Clone()
            {
                return new Node(this.Index, this.Weight);
            }
        }

        static class DataModel
        {
            public static int NodesCount
            {
                get { return Weights.Length; }
            }

            public static int[] Weights = { 76, 95, 57, 0, 57, 38, 76, 190, 33, 66, 33, 33, 57, 57, 57, 0, 76, 66, 190, 57, 57, 76, 285, 114, 133 };
            public static long[,] DistanceMatrix = {
                {0,12507,14912,14912,22005,27078,21570,37100,30206,31270,31709,32312,26220,23628,23979,23979,27337,33879,35990,32918,37894,45211,45895,45874,45610},
                {12550,0,9004,9004,18479,17432,11924,27454,34480,33429,34763,34246,21628,19036,19386,19386,22745,29287,39177,36104,41081,35565,36249,36228,35964},
                {14858,9027,0,0,8409,10713,17294,18817,16610,17673,18113,18715,12624,10032,10382,10382,13740,20283,22394,19321,24298,28042,27568,27444,27122},
                {14858,9027,0,0,8409,10713,17294,18817,16610,17673,18113,18715,12624,10032,10382,10382,13740,20283,22394,19321,24298,28042,27568,27444,27122},
                {21999,19431,8373,8373,0,2873,7803,10977,13297,14360,14800,15402,9311,6718,7069,7069,11280,16970,19081,16008,20985,24728,24255,24130,23808},
                {24204,18285,10577,10577,2774,0,6657,8533,14712,14023,14040,14407,10726,8134,8484,8484,12695,18385,21847,18774,23752,21265,21948,31600,21664},
                {22507,12823,15553,15553,7749,6702,0,17364,24390,23339,24673,24156,15701,13109,13460,13460,17670,23360,29087,26014,30991,25475,26159,26138,25874},
                {38090,28406,22209,22209,10977,8533,18800,0,8617,7566,8900,8383,10941,15711,16061,16061,18813,18530,13314,10241,15218,12732,13415,13671,13130},
                {31968,36738,18342,18342,15049,16564,27132,10049,0,4593,5032,5635,7073,11843,12194,12194,11711,12525,7309,4236,9213,14961,14488,14363,14041},
                {28081,23482,14455,14455,11162,12677,24816,7732,5580,0,950,1317,3187,7957,8307,8307,12518,15493,10277,7204,12182,15616,15142,15018,14696},
                {27677,23077,14050,14050,10758,12273,17203,8044,5892,845,0,780,2782,7552,7903,7903,12114,15805,10590,7517,12494,16016,15542,15418,15096},
                {27231,22632,13605,13605,10313,11827,16757,8086,5934,1556,711,0,2337,7107,7458,7458,11669,15847,10631,7558,12535,18798,18325,18200,17878},
                {26197,21598,12571,12571,9278,10793,15723,12869,6350,7413,7853,8455,0,6073,6424,6424,10229,15757,12134,9061,14038,17782,17308,17184,16862},
                {23653,19054,10027,10027,6735,8249,13179,16579,10060,11123,11563,12165,6074,0,1557,1557,5768,11458,15844,12771,17748,21492,21018,20894,20572},
                {24004,19405,10378,10378,7085,8600,13530,16930,10411,11474,11914,12516,6425,1557,0,0,4992,10682,16195,13122,18099,21842,21369,21244,20922},
                {24004,19405,10378,10378,7085,8600,13530,16930,10411,11474,11914,12516,6425,1557,0,0,4992,10682,16195,13122,18099,21842,21369,21244,20922},
                {28215,23616,14589,14589,11296,12811,17741,19041,11717,13585,14024,14627,10636,5768,4992,4992,0,10690,14561,15233,20210,23953,23480,23355,23033},
                {33924,29325,20298,20298,17006,18520,23450,18672,16627,14666,15105,15708,16345,11477,10701,10701,10709,0,5444,9818,11013,19213,18739,18615,18292},
                {36825,40146,23199,23199,19906,21990,30541,13457,11411,9450,9889,10492,11930,16700,14554,14554,14563,5446,0,4602,5797,13997,13524,13399,13077},
                {36291,39612,22665,22665,19372,21456,30007,12923,10878,8916,9356,9958,11396,16166,16517,16517,17692,8575,3359,0,5263,13463,12990,12865,12543},
                {38889,42210,25263,25263,21970,24054,32605,15521,13476,11514,11954,12556,13994,18764,19115,19115,20441,11324,6108,6666,0,9096,8622,8498,8176},
                {45677,35992,28087,28087,24794,21094,26386,12561,17008,14338,14778,15380,16818,21589,21939,21939,26150,19321,14106,14663,9299,0,908,1164,1667},
                {46325,36641,28051,28051,24758,21743,27035,13209,16972,14302,14742,15344,16782,21552,21903,21903,26114,18833,13617,14175,8810,1143,0,266,1179},
                {46571,36886,28094,28094,24802,21988,27281,13455,17015,14346,14785,15388,16826,21596,21947,21947,26158,18876,13661,14219,8854,1274,268,0,1223},
                {46766,37081,27924,27924,24632,22183,27476,13650,16845,14176,14615,15218,16656,21426,21777,21777,25988,18747,13532,14089,8725,1468,904,775,0},
            };
        }
    }
}