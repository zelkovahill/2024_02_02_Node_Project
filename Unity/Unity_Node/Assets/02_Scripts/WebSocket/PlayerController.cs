using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool isMyPlayer = false;
    public float moveSpeed = 5.0f;

    public void SetAsMyPlayer()
    {
        isMyPlayer = true;
    }

    private void Update()
    {
        // 내 플레이어 일 때 만 움직임
        if (isMyPlayer)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0f, vertical);
            transform.Translate(movement * moveSpeed * Time.deltaTime);
        }
    }
}
