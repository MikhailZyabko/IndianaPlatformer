using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _minGroundNormalY = .65f;
    [SerializeField] private float _gravityModifier = 1f; //Степень влияния гравитации (заменяет тот же множитель у RidgidBody2D)
    [SerializeField] private Vector2 _velocity;
    [SerializeField] private LayerMask _layerMask;

    protected Vector2 targetVelocity; //Ускорение к которому стремимся
    protected bool isGrounded; //True когда на земле
    protected Vector2 groundNormal;
    protected Rigidbody2D rigidBody;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    private void OnEnable()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //Указываем с кем хотим сталкиватся
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(_layerMask);
        contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        targetVelocity = new Vector2(Input.GetAxis("Horizontal"), 0);

        if (Input.GetKey(KeyCode.Space) && isGrounded) // прыжок, надо переделать
            _velocity.y = 5;
    }

    private void FixedUpdate()
    {
        _velocity += _gravityModifier * Physics2D.gravity * Time.deltaTime; //Записываем в передвижение гравитацию умнлженную на модификатор
        _velocity.x = targetVelocity.x; //Присваиваем передвижению по X целевле передвижение по X

        isGrounded = false; //По умолчанию считаем себя не на земле

        Vector2 deltaPosition = _velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x); //Вектор направленный вдоль поверхности
        Vector2 move = moveAlongGround * deltaPosition.x; //Вектор итогового движения

        //В этой части скрипта реализовано движение по "кривым" поверхностям. Сначала по X потом по Y
        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    private void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;
        float projection;
        float modifiedDistance;

        Vector2 currentNormal;

        if (distance > minMoveDistance) //Проверяем делаем ли минимальный шаг
        {
            int count = rigidBody.Cast(move, contactFilter, hitBuffer, distance + shellRadius);

            hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]); //Переписываем столкновения из массива в список
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                currentNormal = hitBufferList[i].normal; //Записываем текущую нормаль препятствия
                if (currentNormal.y > _minGroundNormalY) //Проверяем нормаль по Y препятствия
                {
                    isGrounded = true; //Если нормаль препятствия больше минимальной считаем что это "земля"
                    if(yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                    projection = Vector2.Dot(_velocity, currentNormal);
                    if (projection < 0)
                    {
                        _velocity = _velocity - projection * currentNormal;
                    }
                    modifiedDistance = hitBufferList[i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance; //Проверяем не врезвемся ли в препятствие и сокращаем соответственно ускорение
                }
            }
            rigidBody.position = rigidBody.position + move.normalized * distance;
        }
    }
}
