using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapCameraScript : MonoBehaviour
{
    /*public GameObject focus;
    public float shakeMagnitude;
    public Vector2 shakeInterval;*/
	
	public Texture PlayerTexture;
	public Texture EnemyTexture;
	public Texture TeammateTexture;
	
	private GUIStyle blankStyle;
    

    GameObject[] enemies;
    GameObject player;

    //Random random; //Camera Shaking

	void Start () 
    {
        enemies = MathTester.FindGameObjectsWithLayer(9);
        player = GameObject.FindWithTag("Player");
	}

	void Update () 
    {

        
        
	}

    void FixedUpdate()
    {

    }

    void OnGUI()
    {
		if (gameObject.camera.enabled)
		{
	      	player = GameObject.FindWithTag("Player");
			
			Vector3 screenPos = gameObject.camera.WorldToScreenPoint(new Vector3 (player.transform.position.x, 100, player.transform.position.z));
			
			GUI.DrawTexture(new Rect(screenPos.x - 10, Screen.height - screenPos.y - 10, 20, 20), PlayerTexture);
			
			for (int i = 0; i < enemies.Length; i++)
			{
				screenPos = gameObject.camera.WorldToScreenPoint(new Vector3 (enemies[i].transform.position.x, 100, enemies[i].transform.position.z));
				GUI.DrawTexture(new Rect(screenPos.x - 10, Screen.height - screenPos.y - 10, 20, 20), EnemyTexture);
			}
		}
    }
    
}
