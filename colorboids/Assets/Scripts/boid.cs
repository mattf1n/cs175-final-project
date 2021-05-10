using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class boid : MonoBehaviour
{
  public float speed = 3;
  public float turnSpeed = 1;
  public float proximityTolerance = 1;
  public float sensingDistance = 3;
  public float colorSensingDistance = 1.5f;
  public float leash = 10;
  public float attraction = 3;
  public float separation = 0;
  public float alignment = 0;
  public float cohesion = 0;
  public float avoidance = 5;
  public float colorDiv = 90;
  public Slider separationSlider;
  public Slider alignmentSlider;
  public Slider cohesionSlider;
  public Slider predatorSlider;
  public GameObject attractor;
  public GameObject predator;
  public Color originalColor;


  // Start is called before the first frame update
  void Start()
  {
    separationSlider.onValueChanged.AddListener(delegate {UpdateFlockBehavior();});
    alignmentSlider.onValueChanged.AddListener(delegate {UpdateFlockBehavior();});
    cohesionSlider.onValueChanged.AddListener(delegate {UpdateFlockBehavior();});
    //predatorSlider.onValueChanged.AddListener(delegate {UpdateFlockBehavior(;)});
    UpdateFlockBehavior();

    var objectRenderer = transform.GetComponent<Renderer>();
    originalColor = objectRenderer.material.color;
  }

  void UpdateFlockBehavior() {
    separation = separationSlider.value;
    alignment = alignmentSlider.value;
    cohesion = cohesionSlider.value;
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

    // Find flockmates
    List<Transform> sensedFlockmates = new List<Transform>();
    for (int i = 0; i < transform.parent.childCount; i++) {
      Transform f = transform.parent.GetChild(i); 
      float d = (f.position - transform.position).magnitude;
      if (i != transform.GetSiblingIndex() && d < sensingDistance) {
        sensedFlockmates.Add(f);
      }
    }

    // Cohesion
    int n = sensedFlockmates.Count;
    Vector3 avgSensedPos = new Vector3();
    foreach (Transform f in sensedFlockmates) avgSensedPos += f.position;
    if (n > 0) {
      avgSensedPos /= n;
      Vector3 toFlockmates = avgSensedPos - transform.position;
      force cohesionForce;
      cohesionForce.direction = Quaternion.LookRotation(toFlockmates);
      cohesionForce.weight = cohesion;
      forces.Add(cohesionForce);
    }

    // Alignment
    if (n > 0) {
      float weight = 1f / n;
      Quaternion alignDirection = transform.rotation;
      foreach (Transform f in sensedFlockmates) {
        alignDirection = Quaternion.Slerp(alignDirection, f.rotation, weight);
      }
      force align;
      align.direction = alignDirection;
      align.weight = alignment;
      forces.Add(align);
    }

    // Separation
    int closeFlockmates = 0;
    Vector3 avgPos = new Vector3();
    foreach (Transform f in sensedFlockmates)
    {
      float d = (f.position - transform.position).magnitude;
      if (d < proximityTolerance) {
        closeFlockmates++;
        avgPos += f.position;
      }
    }
    if (closeFlockmates > 0) {
      avgPos /= closeFlockmates;
      Vector3 toFlockmates = transform.position - avgPos;
      force sep;
      sep.direction = Quaternion.LookRotation(toFlockmates);
      sep.weight = separation;
      forces.Add(sep);
    }

    // Avoidance
    Vector3 toPredator = transform.position - predator.transform.position;
    if (toPredator.magnitude < sensingDistance) {
      force avoid;
      avoid.direction = Quaternion.LookRotation(toPredator);
      avoid.weight = avoidance;
      forces.Add(avoid);
    }

    // Apply forces
    float count = forces.Count;
    float coeff = turnSpeed * Time.deltaTime / count;
    foreach (force f in forces) {
      float w = coeff * f.weight;
      transform.rotation = Quaternion.Slerp(transform.rotation, f.direction, w);
    }
    
    // Center attraction to keep boids from escaping
    Vector3 toAttractor = attractor.transform.position - transform.position;
    if (Mathf.Abs(toAttractor.x) > leash 
        || Mathf.Abs(toAttractor.z) > leash
        || Mathf.Abs(toAttractor.y) > leash / 2) {
      Quaternion attract = Quaternion.LookRotation(toAttractor);
      float w = 2 * Time.deltaTime;
      transform.rotation = Quaternion.Slerp(transform.rotation, attract, w);
    }


    // Color 
    float avgRColor = 0.0f;
    float avgGColor = 0.0f;
    float avgBColor = 0.0f;

    List<Transform> colorFlockmates = new List<Transform>();
    for (int i = 0; i < transform.parent.childCount; i++) {
      Transform f = transform.parent.GetChild(i); 
      float d = (f.position - transform.position).magnitude;
      if (d < colorSensingDistance) {
        colorFlockmates.Add(f);
      }
    }

    int numColorFlockmates = colorFlockmates.Count;


    if (numColorFlockmates > 1) {
      foreach (Transform f in colorFlockmates) {
        var objectRenderer = f.GetComponent<Renderer>();
        Color objColor = objectRenderer.material.color;

        avgRColor += objColor[0];
        avgGColor += objColor[1];
        avgBColor += objColor[2];
      }
      avgRColor /= numColorFlockmates;
      avgGColor /= numColorFlockmates;
      avgBColor /= numColorFlockmates;
      foreach(Transform f in colorFlockmates) {
        var objectRenderer = f.GetComponent<Renderer>();

       

        Color objColor = objectRenderer.material.color;
        Color avgColor = new Color(avgRColor, avgBColor, avgGColor);
        float r = (avgColor.r - objColor.r)/colorDiv;
        float g = (avgColor.g - objColor.g)/colorDiv;
        float b = (avgColor.b - objColor.b)/colorDiv;
        Color newColor = new Color(objColor.r + r, objColor.g + g, objColor.b + b);
        objectRenderer.material.SetColor("_Color", newColor);
      }
    }
    else {
      var objectRenderer = transform.GetComponent<Renderer>();
      Color currentColor = objectRenderer.material.color;
      float r = (originalColor.r - currentColor.r) / colorDiv;
      float g = (originalColor.g - currentColor.g) / colorDiv;
      float b = (originalColor.b - currentColor.b) / colorDiv;
      Color newColor = new Color(currentColor.r + r, currentColor.g + g, currentColor.b + b);
      objectRenderer.material.SetColor("_Color", newColor);
    }
  }
}
