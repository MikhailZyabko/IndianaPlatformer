using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _minGroundNormalY = .65f;
    [SerializeField] private float _gravityModifier = 1f; //������� ������� ���������� (�������� ��� �� ��������� � RidgidBody2D)
    [SerializeField] private Vector2 _velocity;
    [SerializeField] private LayerMask _layerMask;

    protected Vector2 targetVelocity; //��������� � �������� ���������
    protected bool isGrounded; //True ����� �� �����
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
        //��������� � ��� ����� �����������
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(_layerMask);
        contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        targetVelocity = new Vector2(Input.GetAxis("Horizontal"), 0);

        if (Input.GetKey(KeyCode.Space) && isGrounded) // ������, ���� ����������
            _velocity.y = 5;
    }

    private void FixedUpdate()
    {
        _velocity += _gravityModifier * Physics2D.gravity * Time.deltaTime; //���������� � ������������ ���������� ���������� �� �����������
        _velocity.x = targetVelocity.x; //����������� ������������ �� X ������� ������������ �� X

        isGrounded = false; //�� ��������� ������� ���� �� �� �����

        Vector2 deltaPosition = _velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x); //������ ������������ ����� �����������
        Vector2 move = moveAlongGround * deltaPosition.x; //������ ��������� ��������

        //� ���� ����� ������� ����������� �������� �� "������" ������������. ������� �� X ����� �� Y
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

        if (distance > minMoveDistance) //��������� ������ �� ����������� ���
        {
            int count = rigidBody.Cast(move, contactFilter, hitBuffer, distance + shellRadius);

            hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]); //������������ ������������ �� ������� � ������
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                currentNormal = hitBufferList[i].normal; //���������� ������� ������� �����������
                if (currentNormal.y > _minGroundNormalY) //��������� ������� �� Y �����������
                {
                    isGrounded = true; //���� ������� ����������� ������ ����������� ������� ��� ��� "�����"
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
                    distance = modifiedDistance < distance ? modifiedDistance : distance; //��������� �� ��������� �� � ����������� � ��������� �������������� ���������
                }
            }
            rigidBody.position = rigidBody.position + move.normalized * distance;
        }
    }
}
