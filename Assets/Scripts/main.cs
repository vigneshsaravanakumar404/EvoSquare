using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;

public class main : MonoBehaviour
{
    // Initial Conditions
    static int herbivoreCount = 15;
    static int foodCount = 15;


    // Game Environment
    int bound = 45;
    
    public GameObject herbivore;
    public GameObject food;

    GameObject[] herbivores = new GameObject[herbivoreCount];
    GameObject[] foods= new GameObject[foodCount];

    System.Random random = new System.Random();
          


    void Start()
    {
        // Randomly Spawn Herbivores
        for (int x = 0; x < herbivoreCount; x++)
        {
            Vector3 herbivorePosition;
            if (random.Next(2) == 0)
            {
                herbivorePosition = new Vector3(random.Next(-bound, bound + 1), 1, (random.Next(0, 2) == 0 ? -1 : 1) * 45);
            }
            else
            {
                herbivorePosition = new Vector3((random.Next(0, 2) == 0 ? -1 : 1) * 45, 1, random.Next(-bound, bound + 1));
            }

            Quaternion rotation = Quaternion.identity;
            GameObject temp = Instantiate(herbivore, herbivorePosition, rotation);
            temp.AddComponent<Rigidbody>();
            temp.GetComponent<Rigidbody>().useGravity = false;
            temp.GetComponent<Rigidbody>().velocity = new Vector3 (0, 0, 5);


            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            temp.GetComponent<Rigidbody>().velocity = randomDirection * 10f;

            temp.transform.rotation = Quaternion.LookRotation(temp.GetComponent<Rigidbody>().velocity, -Vector3.up);




            herbivores[x] = temp;




            

        }

        // Randomly Spawn Food
        for (int x = 0; x < foodCount; x++)
        {
            Vector3 foodPosition;
            Quaternion rotation = Quaternion.Euler(-90, 0, 0);

            do { foodPosition = new Vector3(random.Next(-bound, bound), 1, random.Next(-bound, bound)); }
            while (foodPosition.x >= -12 && foodPosition.x <= 12 && foodPosition.z >= -12 && foodPosition.z <= 12);

            foods[x] = Instantiate(food, foodPosition, rotation);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (GameObject herbivore in herbivores)
        {
            Rigidbody rb = herbivore.GetComponent<Rigidbody>();

            // Calculate the current position of the herbivore
            Vector3 currentPosition = herbivore.transform.position;

            // Move the herbivore in the current direction with the specified speed
            Vector3 newPosition = currentPosition + rb.velocity * Time.deltaTime;

            // Check if the new position exceeds the boundaries
            if (newPosition.x < -50f || newPosition.x > 50f || newPosition.z < -50f || newPosition.z > 50f ||
                (newPosition.x >= -13f && newPosition.x <= 13f && newPosition.z >= -13f && newPosition.z <= 13f))
            {
                // Reflect the herbivore's velocity across the boundary
                ReflectVelocity(rb);
            }

            // Check if the herbivore is close to any food objects
            if (foods.Length > 0)
            {
                foreach (GameObject food in foods)
                {
                    if (food != null && Vector3.Distance(herbivore.transform.position, food.transform.position) < 3f)
                    {
                        // Move towards the food in a straight line
                        Vector3 direction = food.transform.position - herbivore.transform.position;
                        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                        herbivore.transform.rotation = Quaternion.RotateTowards(herbivore.transform.rotation, toRotation, 120f * Time.deltaTime);
                        rb.velocity = direction.normalized * 5f;

                        // Check if the herbivore has reached the food
                        if (Vector3.Distance(herbivore.transform.position, food.transform.position) < 0.2f)
                        {
                            // Destroy the food
                            Destroy(food);
                            SetRandomDirection(rb);
                        }

                        continue;
                    }
                }
            }

            // Move the herbivore in a random direction if no food is nearby
            herbivore.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            herbivore.transform.position += rb.velocity * Time.deltaTime;
        }
    }

    private void ReflectVelocity(Rigidbody rb)
    {
        // Reflect the herbivore's velocity across the boundary
        if (rb.position.x < -50f || rb.position.x > 50f)
        {
            rb.velocity = new Vector3(-rb.velocity.x, 0f, rb.velocity.z);
        }

        if (rb.position.z < -50f || rb.position.z > 50f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, -rb.velocity.z);
        }
    }

    private void SetRandomDirection(Rigidbody rb)
    {
        // Set a random velocity direction for the herbivore
        Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
        rb.velocity = randomDirection * 5f;
    }

}


// To do
// Prevent herbivores from spawining on top of each other
// Make the cubes look at the center

