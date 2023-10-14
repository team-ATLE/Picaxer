using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private CharacterController character;
    private Vector3 direction;

    public float jumpForce = 8f;
    public float gravity = 9.81f * 2f;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        direction = Vector3.zero;
    }

    private void Update()
    {
        direction += Vector3.down * gravity * Time.deltaTime;

        if (character.isGrounded)
        {
            direction = Vector3.down;

             // 키보드 입력 (스페이스바) 및 터치 입력을 모두 체크합니다.
            if (Input.GetButton("Jump") || WasScreenTouched()) {
                direction = Vector3.up * jumpForce;
            }
        }

        character.Move(direction * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) {
            FindObjectOfType<GameManager>().GameOver();
        }
    }
    
    // 화면이 터치되었는지 검사하는 메서드
    private bool WasScreenTouched()
    {
        // 입력이 없으면 false를 반환
        if (Input.touchCount == 0) return false;

        // 첫 번째 터치 입력을 가져옴
        Touch touch = Input.GetTouch(0);

        // 터치가 시작되었을 때만 true를 반환
        return touch.phase == TouchPhase.Began;
    }
}
