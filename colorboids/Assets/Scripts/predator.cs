using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class predator : MonoBehaviour
{
  public float speed = 3;
  public float turnSpeed = 1;
  public float sensingDistance = 3;
  public float leash = 10;
  public float attraction = 3;
  public float pursuit = 0;
  public GameObject attractor;
  public GameObject prey;


  // Start is called before the first frame update
  void Start()
  {
  }

  struct force {
    public Quaternion direction;
    public float weight;
  }

  // Update is called once per frame
  void LateUpdate()
  { 
    transform.Translate(Vector3.forward * speed * Time.deltaTime);

    List<force> forces = new List<force>();

    // Find boids
    List<Transform> sensedBoids = new List<Transform>();
    for (int i = 0; i < prey.transform.childCount; i++) {
      Transform f = prey.transform.GetChild(i); 
      float d = (f.position - transform.position).magnitude;
      if (d < sensingDistance) {
        sensedBoids.Add(f);
      }
    }

    // Pursuit
    int n = sensedBoids.Count;
    Vector3 avgSensedPos = new Vector3();
    foreach (Transform f in sensedBoids) avgSensedPos += f.position;
    if (n > 0) {
      avgSensedPos /= n;
      Vector3 toBoids = avgSensedPos - transform.position;
      force cohesionForce;
      cohesionForce.direction = Quaternion.LookRotation(toBoids);
      cohesionForce.weight = pursuit;
      forces.Add(cohesionForce);
    }

    float count = forces.Count;
    float coeff = turnSpeed * Time.deltaTime / count;
    foreach (force f in forces) {
      float w = coeff * f.weight;
      transform.rotation = Quaternion.Lerp(transform.rotation, f.direction, w);
    }
    
    // Center attraction to keep predator from escaping
    Vector3 toAttractor = attractor.transform.position - transform.position;
    if (Mathf.Abs(toAttractor.x) > leash 
        || Mathf.Abs(toAttractor.z) > leash
        || Mathf.Abs(toAttractor.y) > leash / 2) {
      Quaternion attract = Quaternion.LookRotation(toAttractor);
      float w = 5f * Time.deltaTime;
      transform.rotation = Quaternion.Lerp(transform.rotation, attract, w);
    }
  }
}
