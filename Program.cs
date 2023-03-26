using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using System.Threading;

namespace RNGame
{
    class Vector2
    {
        public int x;
        public int y;

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public double Distance(Vector2 point)
        {
            double d = Math.Sqrt((point.x - this.x) ^ 2 + (point.y - this.y) ^ 2);
            return d;
        }
    }
    class Physical                                                                      //<--- Physical Class Here
    {
        public Map map;
        public Vector2 position = new Vector2(0, 0);
        public string name = "";

        public void Spawn()
        {
            Random rng = new Random();
            do
            {
                position.x = rng.Next(map.preferences.width);
                position.y = rng.Next(map.preferences.height);
            } while (Collide(map.physicals));
            map.physicals.Add(this);
        }

        public bool Collide (List<Physical> colliders)
        {
            foreach (Physical collider in colliders)
            {
                if (collider.position.x == this.position.x && collider.position.y == this.position.y)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Collide(Physical collider)
        {
            if (collider.position.x == this.position.x && collider.position.y == this.position.y)
            {
                return true;
            }
            return false;
        }
    }

    class Chest : Physical
    {
        public int chances;     //0-100

        private Map map;

        Random rng = new Random();
        Enemy mimic;

        public Chest(Map map, int chances)
        {
            this.name = "Chest";
            this.map = map;

            this.chances = Math.Clamp(chances, 0, 100);
        }

        public void Loot()
        {
            Random rng = new Random();
            int c = rng.Next(0, 101);

            if (c > this.chances)
            {
                Console.WriteLine("Looting...");
            }
            else
            {
                map.player.EncounterMessage(this.mimic);
            }
        }
    }
    class LootMember
    {
        public Item item;
        public int chance;

        public LootMember(Item item, int chance)
        {
            this.item = item;
            this.chance = chance;
        }
    }
    class LootTable
    {
        public List<LootMember> loot = new List<LootMember>();

        public LootTable(List<LootMember> loot)
        {
            this.loot = loot;
        }

        private List<Item> ChooseItems(int number)
        {
            Random rng = new Random();

            List<Item> chances = new List<Item>();
            List<Item> items = new List<Item>();

            foreach (LootMember member in loot)
            {
                for (int i = 0; i < member.chance; i++)
                {
                    chances.Add(member.item);
                }
            }

            for (int n = 0; n < number; n++)
            {
                items.Add(chances[rng.Next(chances.Count)]);
            }

            return items;
        }
    }

    class GamePreferences
    {
        public int width;
        public int height;

        public int numOfEnemies;

        public int minDamage;
        public int maxDamage;

        public int minEnemySpeed;
        public int maxEnemySpeed;

        public int numberOfChests;

        public int minChances;
        public int maxChances;

        public void Initialize(int width, int height, int minDamage, int maxDamage, int minEnemySpeed, int maxEnemySpeed, int enemyCount, int numberOfChests)
        {
            this.width = width;
            this.height = height;

            this.numOfEnemies = enemyCount;

            this.minDamage = minDamage;
            this.maxDamage = maxDamage;

            this.numberOfChests = numberOfChests;
        }
    }

    class RandWord
    {
        private Random rng = new Random();

        private string[] consonants = {"b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "z"};
        private string[] vowels = { "a", "e", "i", "o", "u", "y"};

        public string RandomName(int letterLength)
        {
            string name = "";
            for (int i = 0; i < letterLength; i++)
            {
                if (i % 2 == 0)
                {
                    if (i == 0)
                    {
                        name += consonants[rng.Next(consonants.Length)].ToUpper();
                    }
                    else
                    {
                        name += consonants[rng.Next(consonants.Length)];
                    }
                }
                else
                {
                    name += vowels[rng.Next(vowels.Length)];
                }
            }
            return name;
        }   
    }
    class Map
    {
        public Player player;

        public List<Enemy> enemies = new List<Enemy>();

        public List<Physical> physicals = new List<Physical>();

        private Random rng = new Random();

        public GamePreferences preferences;

        public Map(GamePreferences preferences)
        {
            this.preferences = preferences;
        }

        public void Populate()
        {
            for (int c = 0; c < preferences.numOfEnemies; c++)
            {
                Enemy newEnemy = new Enemy(this, rng.Next(this.preferences.minDamage, this.preferences.maxDamage), rng.Next(this.preferences.minEnemySpeed, this.preferences.maxEnemySpeed));
                newEnemy.Spawn();
            }
            for (int c = 0; c < preferences.numberOfChests; c++)
            {
                Chest newChest = new Chest(this, rng.Next(this.preferences.minChances, this.preferences.maxChances));
                newChest.Spawn();
            }
        }

        public Physical AtCoords(Vector2 pos)
        {
            foreach (Physical physical in physicals)
            {
                if (physical.position.x == pos.x && physical.position.y == pos.y)
                {
                    return physical;
                }
            }
            return null;
        }

        public void Display()
        {
            for (int y = 0; y < preferences.height; y++)
            {
                for (int x = 0; x < preferences.width; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    Physical current = AtCoords(pos);
                    if (current == null)
                    {
                        Console.Write("  ");
                        continue;
                    }
                    else
                    {
                        string n = current.name;
                        switch (n)
                        {
                            case "Enemy":
                                Console.Write(@"  ");
                                break;
                            case "Chest":
                                Console.Write(@" x");
                                break;
                            case "Player":
                                Console.Write(@" O");
                                break;
                            default:
                                Console.Write(@"  ");
                                break;
                        }
                    }
                }
                Console.Write("|\n");
            }
            for (int x = 0; x < preferences.width; x++)
            {
                Console.Write("__");
            }
            Console.Write("/\n");
        }
    }
    class PlayerStats
    {
        public int speed;
        public int damage;

        public PlayerStats(int speed, int damage)
        {
            this.speed = speed;
            this.damage = damage;
        }
    }
    class Item
    {
        public string name;
        public string description;
    }
    class Player : Physical
    {
        public enum Direction
        {
            Up,
            Right,
            Down,
            Left
        };

        public int health;
        public int speed;

        public List<Item> inventory = new List<Item>();

        public Player(Map map, int health, int speed, string name)
        {
            this.map = map;
            this.speed = speed;
            this.name = "Player";
            map.player = this;
        }

        public void StartMove(Direction direction, int timePerStep, int numOfSteps)
        {
            for (int i = 0; i < numOfSteps; i++)
            {
                switch (i % 2)
                {
                    case 0:
                        Console.WriteLine(" n\n| |\n\\_/");
                        Thread.Sleep(timePerStep);
                        break;
                    case 1:
                        Console.WriteLine("      n\n     | |\n     \\_/");
                        Thread.Sleep(timePerStep);
                        break;
                }
            }

            this.Move(direction);
            map.Display();
        }

        public void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    position.y -= 1;
                    break;
                case Direction.Right:
                    position.x += 1;
                    break;
                case Direction.Down:
                    position.y += 1;
                    break;
                case Direction.Left:
                    position.x -= 1;
                    break;
                default:
                    break;
            }

            Console.WriteLine($"{position.x}, {position.y}");

            Enemy encounter = this.Encounter();
            if (encounter != null)
            {
                EncounterMessage(encounter);
            }
        }

        public Enemy Encounter()
        {
            foreach (Physical physical in map.physicals)
            {
                if (this.Collide(physical) && physical.name == "Enemy")
                {
                    return (Enemy)physical;
                }
            }
            return null;
        }

        public void EncounterMessage(Enemy enemy)
        {
            Console.WriteLine($"\nYou've Encountered {enemy.enemyName}!\n");
            Console.WriteLine("What do you do?\n1. Attempt Flee\n2. Begin Combat");
            Console.Write(">");
            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    if (AttemptFlee(enemy))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Console.Write(".");
                            Thread.Sleep(600);
                        }
                        Console.Write("\n");
                    }
                    break;
                case "2":
                    break;
                default:
                    Console.WriteLine("Invalid Response...");
                    EncounterMessage(enemy);
                    break;
            }
        }

        private bool AttemptFlee(Enemy enemy)
        {
            //returns "can i flee?"
            if (enemy.speed < this.speed)
            {
                map.physicals.Remove(enemy);
                return true;
            }
            return false;
        }

        public Direction GetDirectionFromNumber(int direction)
        {
            Direction newDirection = Direction.Up;
            switch (direction)
            {
                case 1:
                    newDirection = Direction.Up;
                    break;
                case 2:
                    newDirection = Direction.Right;
                    break;
                case 3:
                    newDirection = Direction.Down;
                    break;
                case 4:
                    newDirection = Direction.Left;
                    break;
            }
            return newDirection;
        }
    }
    class Enemy : Physical
    {
        public int damage;
        public int health;
        public int precision;
        public int speed;

        public string enemyName;

        private Random rng = new Random();
        private RandWord rngWord = new RandWord();

        public Enemy(Map map, int damage, int speed)
        {
            this.damage = damage;
            this.speed = speed;
            this.map = map;
            this.name = "Enemy";
            this.enemyName = rngWord.RandomName(6);
        }

        public bool AtCoords(int x, int y)
        {
            if (x == this.position.x && y == this.position.y)
            {
                return true;
            }
            return false;
        }
    }

    class Program
    {
        static private GamePreferences NewPreferences()
        {
            GamePreferences preferences = new GamePreferences();

            Console.Write("map width: ");
            int mapx = Convert.ToInt32(Console.ReadLine());
            Console.Write("map height: ");
            int mapy = Convert.ToInt32(Console.ReadLine());
            Console.Write("minimum enemy damage: ");
            int minDamage = Convert.ToInt32(Console.ReadLine());
            Console.Write("maximum enemy damage: ");
            int maxDamage = Convert.ToInt32(Console.ReadLine());
            Console.Write("minimum enemy speed: ");
            int minSpeed = Convert.ToInt32(Console.ReadLine());
            Console.Write("maximum enemy speed: ");
            int maxSpeed = Convert.ToInt32(Console.ReadLine());
            Console.Write("enemies: ");
            int enemyCount = Int32.Parse(Console.ReadLine());
            Console.Write("chests: ");
            int chests = Int32.Parse(Console.ReadLine());

            preferences.Initialize(mapx, mapy, minDamage, maxDamage, minSpeed, maxSpeed, enemyCount, chests);

            Console.WriteLine("Would you like to save these preferences? (Y/N)");
            Console.Write(">");
            string saveprompt = Console.ReadLine();

            switch(saveprompt)
            {
                case "Y":
                    Console.Write("file name: ");
                    string name = Console.ReadLine();
                    string json = JsonConvert.SerializeObject(preferences);
                    using (StreamWriter sw = new StreamWriter($"./preferences/{name}.json"))
                    {
                        sw.Write(json);
                    }
                    break;
                case "N":
                    break;
            }

            return preferences;
        }

        static void Main(string[] args)
        {
            System.IO.Directory.CreateDirectory("./preferences/");

            Console.WriteLine("Load existing preferences? (Y/N)");
            Console.Write(">");
            string loadfileprompt = Console.ReadLine();

            GamePreferences preferences = new GamePreferences();
            
            switch (loadfileprompt.ToLower())
            {
                case "y":
                    Console.Write("file name: ");
                    string name = Console.ReadLine();
                    string json;
                    using (StreamReader sr = new StreamReader($"./preferences/{name}.json"))
                    {
                        json = sr.ReadToEnd();
                        preferences = JsonConvert.DeserializeObject<GamePreferences>(json);
                    }
                    break;
                case "n":
                    preferences = NewPreferences();
                    break;
                default:
                    break;
            }

            Map map = new Map(preferences);
            map.Populate();

            Player player = new Player(map, 50, 2, "Player");
            player.Spawn();

            map.Display();

            //Game Starts Here
            bool playing = true;
            while (playing)
            {
                Console.Write(">");
                string input = Console.ReadLine();

                switch (input.ToLower())
                {
                    case "move":
                        Console.WriteLine("   1. Up\n   2. Right\n   3. Down\n   4. Left");
                        int direction = Int32.Parse(Console.ReadLine());
                        player.StartMove(player.GetDirectionFromNumber(direction), 300, 3);
                        break;

                }
            }
        }
    }
}
