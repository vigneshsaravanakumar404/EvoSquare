using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Main : MonoBehaviour
{
    // Initial Conditions
    static int herbivoreCount = 20;
    static int foodCount = 10;
    float minimumDistance = 2.5f;

    // Game Environment
    int bound = 45;
    public GameObject herbivore;
    public GameObject food;

    // Lists
    List<int> energyLevels = new List<int>();
    List<float> speeds = new List<float>();
    List<float> sizes = new List<float>();
    List<float> visions = new List<float>();
    List<GameObject> herbivores = new List<GameObject>();
    HashSet<Vector3> validPositions = new HashSet<Vector3>();
    List<GameObject> foods = new List<GameObject>();

    void Start()
    {
        
        for (int x = 0; x < herbivoreCount; x++)
        {
            // Variables
            Vector3 herbivorePosition;
            Quaternion rotation = Quaternion.identity;
            Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
            float speed = UnityEngine.Random.Range(4f, 6f);
            float vision = UnityEngine.Random.Range(2f, 4f);
            float size = UnityEngine.Random.Range(0.5f, 1.5f);

            bool isValidPosition = false;
            int attemptCount = 0;

            do
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    herbivorePosition = new Vector3(UnityEngine.Random.Range(-bound, bound + 1), size, (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * 45);
                }
                else
                {
                    herbivorePosition = new Vector3((UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * 45, size, UnityEngine.Random.Range(-bound, bound + 1));
                }

                isValidPosition = IsPositionValid(herbivorePosition, minimumDistance);

                attemptCount++;

                // Infinite loop preventer
                if (attemptCount >= 100)
                {
                    Debug.LogWarning("Failed to find a valid position for herbivore within the specified distance");
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        herbivorePosition = new Vector3(UnityEngine.Random.Range(-bound, bound + 1), size, (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * bound);
                    }
                    else
                    {
                        herbivorePosition = new Vector3((UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * bound, size, UnityEngine.Random.Range(-bound, bound + 1));
                    }

                    isValidPosition = true; // Mark the position as valid since it is on the edge
                }

            } while (!isValidPosition);

            // Create new herbivore with random speed/size/vision
            if (isValidPosition)
            {
                // Initialize New variables
                GameObject temp = Instantiate(herbivore, herbivorePosition, rotation);
                Renderer cubeRenderer = temp.transform.Find("Cube").GetComponent<Renderer>();
                Color color = CalculateColor(speed, vision, size);
                cubeRenderer.material.color = color;

                // Calculations
                Rigidbody rb = temp.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.velocity = randomDirection * speed;
                temp.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
                temp.transform.rotation *= Quaternion.Euler(0, -90, 0);
                temp.transform.localScale = new Vector3(size, size, size);

                // Storage
                speeds.Add(speed);
                visions.Add(vision);
                sizes.Add(size);
                herbivores.Add(temp);
                energyLevels.Add(5);
                validPositions.Add(herbivorePosition);
            }
        }

        // Randomly Spawn Food
        for (int x = 0; x < foodCount; x++)
        {
            Vector3 foodPosition;
            Quaternion rotation = Quaternion.Euler(-90, 0, 0);

            do
            {
                foodPosition = new Vector3(UnityEngine.Random.Range(-bound, bound), 1, UnityEngine.Random.Range(-bound, bound));
            } while (foodPosition.x >= -12 && foodPosition.x <= 12 && foodPosition.z >= -12 && foodPosition.z <= 12);

            GameObject foodObject = Instantiate(food, foodPosition, rotation);
            foods.Add(foodObject);
        }
    }

    private void Update()
    {
        foreach (GameObject herbivore in herbivores)
        {
            Rigidbody rb = herbivore.GetComponent<Rigidbody>();

            // Calculations
            Vector3 currentPosition = herbivore.transform.position;
            Vector3 newPosition = currentPosition + rb.velocity * Time.deltaTime;

            if (newPosition.x < -46f || newPosition.x > 46 || newPosition.z < -46 || newPosition.z > 46 ||
                (newPosition.x >= -13f && newPosition.x <= 13f && newPosition.z >= -13f && newPosition.z <= 13f))
            {
  
                ReflectVelocity(rb);

                if (newPosition.x >= -13f && newPosition.x <= 13f && newPosition.z >= -13f && newPosition.z <= 13f)
                {
                    rb.velocity = -rb.velocity;
                }
            }

            // Check if the herbivore is close to any food objects
            if (foods.Count > 0)
            {
                foreach (GameObject food in foods.ToList())
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

                            // Increase the energy level of the herbivore
                            int herbivoreIndex = herbivores.IndexOf(herbivore);
                            energyLevels[herbivoreIndex]++;

                            // Remove the food from the list
                            foods.Remove(food);
                        }

                        continue;
                    }
                }
            }

            // Move the herbivore in a random direction if no food is nearby
            herbivore.transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
            herbivore.transform.rotation *= Quaternion.Euler(0, -90, 0);
            herbivore.transform.position += rb.velocity * Time.deltaTime;

            // Update color based on attributes
            Renderer cubeRenderer = herbivore.transform.Find("Cube").GetComponent<Renderer>();
            Color color = CalculateColor(speeds[herbivores.IndexOf(herbivore)], visions[herbivores.IndexOf(herbivore)], sizes[herbivores.IndexOf(herbivore)]);
            cubeRenderer.material.color = color;
        }
        
        if (foods.Count == 0)
        {
            for (int x = 0; x < herbivores.Count; x++)
            {
                energyLevels[x]--;
                int energyLevel = energyLevels[x];

                if (energyLevel < 0)
                {
                    // Delete the herbivore at the xth index
                    Destroy(herbivores[x]);

                    // Remove the attributes from the respective lists
                    energyLevels.RemoveAt(x);
                    speeds.RemoveAt(x);
                    visions.RemoveAt(x);
                    sizes.RemoveAt(x);
                    herbivores.RemoveAt(x);

                    x--; // Decrement x since the herbivore at the current index is removed

                    continue; // Skip the remaining code for this iteration
                }
                else if (energyLevel > 2)
                {
                    //TODO Reproduction Code 
                }

                // Reset Simulation
                Vector3 herbivorePosition;
                Quaternion rotation = Quaternion.identity;
                Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
                float speed = speeds[x];
                float vision = visions[x];
                float size = sizes[x];
                bool isValidPosition = false;
                int attemptCount = 0;

                do
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        herbivorePosition = new Vector3(UnityEngine.Random.Range(-bound, bound + 1), size, (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * 45);
                    }
                    else
                    {
                        herbivorePosition = new Vector3((UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * 45, size, UnityEngine.Random.Range(-bound, bound + 1));
                    }

                    isValidPosition = IsPositionValid(herbivorePosition, minimumDistance);
                    attemptCount++;
                    

                    // Infinite loop preventer
                    if (attemptCount >= 100)
                    {
                        Debug.LogWarning("Failed to find a valid position for herbivore within the specified distance");
                        // Place the herbivore randomly on the edges
                        if (UnityEngine.Random.Range(0, 2) == 0)
                        {
                            herbivorePosition = new Vector3(UnityEngine.Random.Range(-bound, bound + 1), size, (UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * bound);
                        }
                        else
                        {
                            herbivorePosition = new Vector3((UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1) * bound, size, UnityEngine.Random.Range(-bound, bound + 1));
                        }

                        isValidPosition = true;
                    }

                } while (!isValidPosition);

                if (isValidPosition)
                {
                    herbivores[x].transform.position = herbivorePosition;
                    herbivores[x].transform.rotation = rotation;
                    Rigidbody rb = herbivores[x].GetComponent<Rigidbody>();
                    rb.velocity = randomDirection * speed;
                    herbivores[x].transform.rotation = Quaternion.LookRotation(rb.velocity, Vector3.up);
                    herbivores[x].transform.rotation *= Quaternion.Euler(0, -90, 0);
                    herbivores[x].transform.localScale = new Vector3(size, size, size);

                    // Update the valid positions set
                    validPositions.Remove(herbivorePosition);
                    validPositions.Add(herbivorePosition);
                }
            }

            // Randomly Spawn Food
            for (int x = 0; x < foodCount; x++)
            {
                Vector3 foodPosition;
                Quaternion rotation = Quaternion.Euler(-90, 0, 0);

                do
                {
                    foodPosition = new Vector3(UnityEngine.Random.Range(-bound, bound), 1, UnityEngine.Random.Range(-bound, bound));
                } while (foodPosition.x >= -12 && foodPosition.x <= 12 && foodPosition.z >= -12 && foodPosition.z <= 12);

                GameObject foodObject = Instantiate(food, foodPosition, rotation);
                foods.Add(foodObject);
            }

            

        }

    }

    // Bounce off walls and pond
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

    // Set random direction
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

    // Check valid positions
    bool IsPositionValid(Vector3 position, float minimumDistance)
    {
        foreach (Vector3 validPosition in validPositions)
        {
            if (Vector3.Distance(validPosition, position) < minimumDistance)
            {
                return false;
            }
        }
        return true;
    }

    // Calculate the color
    Color CalculateColor(float speed, float vision, float size)
    {
        // Find the maximum and minimum values of speed, vision, and size
        float maxSpeed = Mathf.Max(speeds.ToArray());
        float minSpeed = Mathf.Min(speeds.ToArray());
        float maxVision = Mathf.Max(visions.ToArray());
        float minVision = Mathf.Min(visions.ToArray());
        float maxSize = Mathf.Max(sizes.ToArray());
        float minSize = Mathf.Min(sizes.ToArray());

        // Calculate normalized values for speed, vision, and size
        float normalizedSpeed = (speed - minSpeed) / (maxSpeed - minSpeed);
        float normalizedVision = (vision - minVision) / (maxVision - minVision);
        float normalizedSize = (size - minSize) / (maxSize - minSize);

        // Calculate the color based on the normalized attributes
        Color color = new Color(normalizedSpeed, normalizedVision, normalizedSize);
        return color;
    }
}


/* To dos:
    Make multiple waves happen (Time To do: High) [Reproduction, energy levels]
     - Set a delay between waves
     - Reproduction
     - Function relating energy cost and gain
    Change color based on the vision and the speed of the herbivore (Time To do: Medium)
    fix bounds again
    Turn constants into variables (average initial Speed, average initial size, average initial vision) so it can 
        be changed in the UI

    Integrate UI (Tejas)
    Integrate Graph (Aryan) 

    Github readme
    Demo video 
    Slide presentation
*/


/* Potential Extras
    Make many attributes and let the user choose 3 out of the x attribues (I think this will be hard)
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
    Make the cubes blink
    Make the day and night cycle change
    sound effects
 */
