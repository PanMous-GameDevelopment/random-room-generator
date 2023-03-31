using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    public GameObject[] walls; // 0 - Up 1 -Down 2 - Right 3- Left
    public GameObject[] doors; // 0 - Up 1 -Down 2 - Right 3- Left

    // Updates the state of the room based on the given boolean values.
    // The status array contains a boolean value for each door.
    public void UpdateRoom(bool[] status)
    {
        
        for (int i = 0; i < status.Length; i++) // Loop through each status in the array.
        {           
            doors[i].SetActive(status[i]); // Set the active state of the door object based on the boolean value.
            walls[i].SetActive(!status[i]); // Set the active state of the wall object based on the opposite of the boolean value.
        }
    }
}

