using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class FoodStockpileArea : MonoBehaviour
{
    private MeshRenderer mesh;
    private Collider trigger;
    public TMP_Text label;
    private Food[] oldFoodInArea;

    public UnityEvent foodEnterEvent;
    public UnityEvent foodExitEvent;

    private void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        trigger = GetComponent<Collider>();
        // mesh.enabled = false;
    }

    private void Update()
    {
        label.text = $"Food collected: {GetFoodCount()}";

        // Detect when food is eaten from within the area
        if (oldFoodInArea != null)
        {
            foreach (Food food in oldFoodInArea)
            {
                if (food.isEaten() && foodExitEvent != null)
                {
                    foodExitEvent.Invoke();
                }
            }
        }

        oldFoodInArea = GetFoodInArea();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Food.IsFood(other.gameObject) && foodEnterEvent != null)
        {
            foodEnterEvent.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Food.IsFood(other.gameObject) && foodExitEvent != null)
        {
            foodExitEvent.Invoke();
        }
    }

    public Food[] GetFoodInArea()
    {
        Food[] foodArr = Food.GetFood();
        List<Food> foodInArea = new List<Food>();

        foreach (Food food in foodArr)
        {
            if (!food.isEaten() && mesh.bounds.Contains(food.transform.position))
            {
                foodInArea.Add(food);
            }
        }

        return foodInArea.ToArray();
    }

    public int GetFoodCount()
    {
        return GetFoodInArea().Length;
    }
}
