using UnityEngine;

// Importante esse script não é o script real de movimento do jogador é somente para eu testar o jogo.

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Velocidade de movimento do jogador

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        // Pega a referência do componente Rigidbody2D no mesmo objeto
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Captura o input do teclado (Setas ou WASD)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Cria um vetor com a direção do input e o normaliza
        // Normalizar garante que o jogador não se mova mais rápido nas diagonais
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // Aplica o movimento ao Rigidbody2D no FixedUpdate para uma física mais consistente
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}