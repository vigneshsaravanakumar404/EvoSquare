using UnityEngine;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    // Initial Conditions
    static int herbivoreCount = 46;
    static int foodCount = 1;

    // Game Environment
    int bound = 45;

    public GameObject herbivore;
    public GameObject food;

    List<int> energyLevels = new();
    List<float> speeds = new();
    List<float> sizes = new();
    List<float> visions = new();
    List<GameObject> herbivores = new();
    List<GameObject> foods = new();

    System.Random random = new();

    void Start()
    {
        float minimumDistance = 2.5f; // Minimum distance between herbivores

        for (int x = 0; x < herbivoreCount; x++)
        {
            // Variables
            Vector3 herbivorePosition;
            Quaternion rotation = Quaternion.identity;
            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            float speed = (float)(random.NextDouble() * 2 + 4);
            float vision = (float)(random.NextDouble() * 2 + 2);
            float size = (float)(random.NextDouble() * 1 + 0.5);

            bool isValidPosition = false;
            int attemptCount = 0;
            do
            {
                if (random.Next(2) == 0)
                {
                    herbivorePosition = new Vector3(random.Next(-bound, bound + 1), size, (random.Next(0, 2) == 0 ? -1 : 1) * 45);
                }
                else
                {
                    herbivorePosition = new Vector3((random.Next(0, 2) == 0 ? -1 : 1) * 45, size, random.Next(-bound, bound + 1));
                }

                isValidPosition = IsPositionValid(herbivorePosition, minimumDistance);

                attemptCount++;
                // Had to put this in here because infinite loops cause unity to crash
                if (attemptCount >= 100)
                {
                    Debug.LogWarning("Failed to find a valid position for herbivore within the specified distance");
                    break;
                }
            }
            while (!isValidPosition);

            // Create new herbivore with random speed/size/vision
            if (isValidPosition)
            {
                GameObject temp = Instantiate(herbivore, herbivorePosition, rotation);
                temp.AddComponent<Rigidbody>();
                temp.GetComponent<Rigidbody>().useGravity = false;
                temp.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 1);
                temp.GetComponent<Rigidbody>().velocity = randomDirection * speed;
                temp.transform.rotation = Quaternion.LookRotation(temp.GetComponent<Rigidbody>().velocity, -Vector3.up);

                // Rotate the herbivore by 90 degrees in the y-axis
                temp.transform.rotation = Quaternion.Euler(90, 90, 90);

                // Scale the size of the herbivore
                temp.transform.localScale = new Vector3(size, size, size);

                speeds.Add(speed);
                visions.Add(vision);
                sizes.Add(size);
                herbivores.Add(temp);
            }

        }



        // Randomly Spawn Food
        for (int x = 0; x < foodCount; x++)
        {
            Vector3 foodPosition;
            Quaternion rotation = Quaternion.Euler(-90, 0, 0);

            do
            {
                foodPosition = new Vector3(random.Next(-bound, bound), 1, random.Next(-bound, bound));
            }
            while (foodPosition.x >= -12 && foodPosition.x <= 12 && foodPosition.z >= -12 && foodPosition.z <= 12);

            GameObject foodObject = Instantiate(food, foodPosition, rotation);
            foods.Add(foodObject);
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

            // Check if the new position exceeds the boundaries or enters the restricted region
            if (newPosition.x < -46f || newPosition.x > 46 || newPosition.z < -46 || newPosition.z > 46 ||
                (newPosition.x >= -13f && newPosition.x <= 13f && newPosition.z >= -13f && newPosition.z <= 13f))
            {
                // Reflect the herbivore's velocity across the boundary
                ReflectVelocity(rb);

                // Additional check for restricted region
                if (newPosition.x >= -13f && newPosition.x <= 13f && newPosition.z >= -13f && newPosition.z <= 13f)
                {
                    // Reverse the herbivore's velocity to prevent entering the restricted region
                    rb.velocity = -rb.velocity;
                }
            }

            // Check if the herbivore is close to any food objects
            if (foods.Count > 0)
            {
                foreach (GameObject food in foods)
                {
                    if (food != null && Vector3.Distance(herbivore.transform.position, food.transform.position) < visions[herbivores.IndexOf(herbivore)])
                    {
                        // Move towards the food in a straight line
                        Vector3 direction = food.transform.position - herbivore.transform.position;
                        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                        herbivore.transform.rotation = Quaternion.RotateTowards(herbivore.transform.rotation, toRotation, 120f * Time.deltaTime);
                        herbivore.transform.rotation = Quaternion.Euler(0, -90, 0);
                        float herbivoreSpeed = speeds[herbivores.IndexOf(herbivore)];
                        rb.velocity = Vector3.ClampMagnitude(direction.normalized * herbivoreSpeed, herbivoreSpeed);


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
            herbivore.transform.rotation *= Quaternion.Euler(0, -90, 0);
            herbivore.transform.position += rb.velocity * Time.deltaTime;
        }
    }


    private void ReflectVelocity(Rigidbody rb)
    {
        // Reflect the herbivore's velocity across the boundary
        if (rb.position.x < -46f || rb.position.x > 46f)
        {
            rb.velocity = new Vector3(-Mathf.Clamp(rb.velocity.x, -5f, 5f), 0f, rb.velocity.z);
        }

        if (rb.position.z < -46f || rb.position.z > 46f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, -Mathf.Clamp(rb.velocity.z, -5f, 5f));
        }
    }

    private void SetRandomDirection(Rigidbody rb)
    {
        // Set a random velocity direction for the herbivore
        Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;

        // Check if a herbivore is created
        int herbivoreIndex = herbivores.IndexOf(rb.gameObject);
        if (herbivoreIndex != -1)
        {
            float herbivoreSpeed = speeds[herbivoreIndex];
            rb.velocity = randomDirection * herbivoreSpeed;
        }
    }


    bool IsPositionValid(Vector3 position, float minimumDistance)
    {
        foreach (GameObject herbivore in herbivores)
        {
            if (Vector3.Distance(herbivore.transform.position, position) < minimumDistance)
            {
                return false;
            }
        }
        return true;
    }

}


/* To dos:

Make multiple waves happen (Time To do: High) [Reproduction, death, energy levels]
Change color based on the vision and the speed of the herbivore (Time To do: Medium)
fix y-position so the object doesn't go through the ground (Time To do: Low)

Integrate UI (Tejas)
Integrate Graph (Aryan) 


Github readme
Demo video 
Slide presentation
*/


/* Potential Extras

Animations when turing
Particles when eating
Animations when eating
Water
Eating other cubes
Strength
Improve scene where there are more trees cause the camera can still see blank stuff
sky
camera rotaes around the clearning 
camera can be moved by the user 
Generate a file output that can be viwed

 */
