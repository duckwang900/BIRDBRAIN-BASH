using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int side1Score = 0;
    public int side2Score = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        side1Score = 0;
        side2Score = 0;
    }

    // checking to see if the ball hits the court on either side
    void OnCollisionEnter(Collision collision)
    {
        // if it touches side 1, then side 2 scores
        if (collision.gameObject.CompareTag("Side1"))
        {
            side2Score += 1;
            Debug.Log("side 2 scored! points: " + side2Score);
            CheckWin();
        } 
        // if it touches side 2, then side 1 scores
        else if (collision.gameObject.CompareTag("Side2")) 
        {
            side1Score += 1;
            Debug.Log("side 1 scored! points: " + side1Score);
            CheckWin();
        }
    }

    // after each score, check the win conditions for both sides
    void CheckWin()
    {
        if (side1Score >= 3 && side1Score > side2Score + 2)
        {
            Debug.Log("side 1 wins! final score: " + side1Score + " to " + side2Score);
        } 
        if (side2Score >= 3 && side2Score > side1Score + 2)
        {
            Debug.Log("side 2 wins! final score: " + side1Score + " to " + side2Score);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
