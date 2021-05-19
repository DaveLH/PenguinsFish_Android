using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animate turning the key in the "RoboPenguin" (AI)
/// </summary>
/// <remarks>
/// OWNER: AI "RoboPenguin"
/// </remarks>
/// 
public class TurnKey : MonoBehaviour
{
    Transform m_trKey;    // Ref to the key object (child of AI Penguin)

    [SerializeField] float KeyRotSpeed = 1.0f;   // Rate per sec. key should rotate

    // Is key currently turning?
    //
	public bool KeyAnimating { get; set; }

	void Start ()
    {
        m_trKey = transform.Find("WindUpKey");

        KeyAnimating = false;
    }
	
    // Rotate the key
    //
    void UpdateRotateKey()
    {
        m_trKey.Rotate(m_trKey.forward, KeyRotSpeed * Time.deltaTime, Space.World);

    }
	
	void Update ()
    {
        if (KeyAnimating) UpdateRotateKey();
	}
}
