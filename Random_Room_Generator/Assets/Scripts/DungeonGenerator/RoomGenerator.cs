using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public class Room // Custom class that defines the properties of a room.
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }

    [System.Serializable]
    public class RoomData // Contains the room data.
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        public bool isObligatory;

        public int SpawnProbability(int x, int y) // Calculates the probability of spawning the room at a given position.
        {
            // 0 = cannot spawn, 1 = can spawn, 2 = HAS to spawn.

            if (x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
            {
                if (isObligatory) return 2; else return 1;
            }

            return 0;
        }

    }

    public Vector2Int size;
    public int startPos = 0;
    public RoomData[] rooms;
    public Vector2 offset;

    List<Room> board;
    void Start()
    {
        DungeonGenerator();
    }

    // Creates the actual 3D rooms based on the dungeon layout created by the DungeonGenerator().
    void GenerateDungeon()
    {
        // Loops through each position in the grid and instantiates the appropriate room type.
        int gridLength = size.x * size.y;
        for (int i = 0; i < gridLength; i++)
        {
            Room currentRoom = board[i]; // Get the current room at this position.

            if (currentRoom.visited)
            {
                int x = i % size.x; // Calculate the x position based on the current index.
                int y = i / size.x; // Calculate the y position based on the current index.

                int randomRoom = -1;
                List<int> availableRooms = new List<int>();

                for (int j = 0; j < rooms.Length; j++) // Loops through each possible room in the list.
                {
                    int p = rooms[j].SpawnProbability(x, y); // Calculates the probability of spawning the room at this position.

                    // Checks the importance of the room, if it can or must spawn.
                    if (p == 2)
                    {
                        randomRoom = j;
                        break;
                    }
                    else if (p == 1)
                    {
                        availableRooms.Add(j);
                    }
                }

                if (randomRoom == -1) // If no room type is mandatory at this position.
                {
                    // If there are available room types that can spawn at this position, choose one randomly.
                    if (availableRooms.Count > 0)
                    {
                        randomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                    }
                    else
                    {
                        randomRoom = 0;
                    }
                }

                // Instantiate the chosen room type at the current position based on the RoomBehaviour script.
                var newRoom = Instantiate(rooms[randomRoom].room, new Vector3(x * offset.x, 0, -y * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                newRoom.UpdateRoom(currentRoom.status);
                newRoom.name += " " + x + "-" + y;
            }
        }


    }

    // Generates the layout of the dungeon. It implements a depth-first search backtracting algorithm to generate a random dungeon layout.
    void DungeonGenerator()
    {
        board = new List<Room>(); // Represents the dungeon layout.
        // Creates size.x * size.y number of rooms and adds them to the board list.
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Room());
            }
        }

        int currentRoom = startPos; // Sets the starting room.

        Stack<int> path = new Stack<int>(); // Creates a stack to keep track of visited rooms.

        int k = 0;

        // Runs the algorithm until it has visited all rooms or has reached a maximum number of iterations.
        while (k < 1000) // This ensures that the loop will eventually stop and wont crush the application. Addapt the value if a bigger dungeon size is needed.
        {
            k++;

            board[currentRoom].visited = true; // Marks the current room as visited.

            // If the current room is the last room, stop generating the dungeon.
            if (currentRoom == board.Count - 1)
            {
                break;
            }

            //Check the room's neighbors
            List<int> neighbors = Check(currentRoom);

            // If all the neighbors are visited then backtrack to the last visited room.
            if (neighbors.Count == 0)
            {
                if (path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentRoom = path.Pop(); // Backtracting to the previous room. The Pop() method removes and returns the top element (the last visited room) from the stack and assigns it to the currentRoom variable.
                }
            }
            else
            {
                path.Push(currentRoom); // Push the current room to the top of the "path" stack so that the algorithm can later backtrack to it when needed.

                int newRoom = neighbors[Random.Range(0, neighbors.Count)]; // Choose a random unvisited neighbor to move to.

                // Update the current room's status based on which neighbor was chosen.
                if (newRoom > currentRoom)
                {
                    // Down or right.
                    if (newRoom - 1 == currentRoom)
                    {
                        board[currentRoom].status[2] = true;
                        currentRoom = newRoom;
                        board[currentRoom].status[3] = true;
                    }
                    else
                    {
                        board[currentRoom].status[1] = true;
                        currentRoom = newRoom;
                        board[currentRoom].status[0] = true;
                    }
                }
                else
                {
                    // Up or left.
                    if (newRoom + 1 == currentRoom)
                    {
                        board[currentRoom].status[3] = true;
                        currentRoom = newRoom;
                        board[currentRoom].status[2] = true;
                    }
                    else
                    {
                        board[currentRoom].status[0] = true;
                        currentRoom = newRoom;
                        board[currentRoom].status[1] = true;
                    }
                }

            }

        }
        GenerateDungeon();
    }

    // Checks a room's neighbors to see which ones are not visited.
    // Checks every side of the room.
    List<int> Check(int room)
    {
        List<int> neighbors = new List<int>();

        int roomRow = room / size.x; // Get the row of the current room.
        int roomCol = room % size.x; // Get the column of the current room.

        // Check the up neighbor.
        if (roomRow > 0 && !board[room - size.x].visited)
        {
            int upRoom = room - size.x; // Get the index of the up room.
            neighbors.Add(upRoom); // Add the up room to the list of neighbors.
        }

        // Check the down neighbor.
        if (roomRow < size.y - 1 && !board[room + size.x].visited)
        {
            int downRoom = room + size.x; // Get the index of the down room.
            neighbors.Add(downRoom); // Add the down room to the list of neighbors.
        }

        // Check the right neighbor.
        if (roomCol < size.x - 1 && !board[room + 1].visited)
        {
            int rightRoom = room + 1; // Get the index of the right room.
            neighbors.Add(rightRoom); // Add the right room to the list of neighbors.
        }

        // Check the left neighbor.
        if (roomCol > 0 && !board[room - 1].visited)
        {
            int leftRoom = room - 1; // Get the index of the left room.
            neighbors.Add(leftRoom); // Add the left room to the list of neighbors.
        }

        return neighbors;
    }

}

