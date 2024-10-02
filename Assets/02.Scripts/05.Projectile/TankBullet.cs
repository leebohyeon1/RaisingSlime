    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TankBullet : Bullet
    {

        public float gravity = 9.81f; // �ϰ��� �� ����� ���ӵ�
        public float fallDistance = 3f; // �÷��̾���� �Ÿ��� �� ������ �۾����� ��ź�� �������� ����
    
        private Vector3 targetPos = Vector3.zero; // Ÿ���� �Ǵ� �÷��̾� �Ǵ� ������
        private bool isFalling = false;

        // Start is called before the first frame update
        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            lifeTimer = lifetime; // ���� Ÿ�̸� �ʱ�ȭ
        }

        protected override void Start()
        {
        }

        protected override void Update()
        {
            base.Update();

            if (!isFalling)
            {
                float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPos.x, 0, targetPos.z));

                // �÷��̾���� �Ÿ��� ������ fallDistance���� �۾����� �ϰ� ����
                if (distanceToPlayer <= fallDistance)
                {
                    isFalling = true;
                    StartFalling();
                }
            }
        }

        public void InitialTarget(Vector3 target)
        {
            targetPos = target;
      
        }

        // ���� �Ÿ� �̻� �÷��̾�� ��������� ��ź�� �������� �����ϴ� �ڷ�ƾ
        IEnumerator CheckFallDistance()
        {
            while (!isFalling)
            {
                if (targetPos != Vector3.zero)
                {
                    float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x,0, transform.position.z), new Vector3(targetPos.x,0, targetPos.z));

   

                    // �÷��̾���� �Ÿ��� ������ fallDistance���� �۾����� �ϰ� ����
                    if (distanceToPlayer <= fallDistance)
                    {
                        isFalling = true;
                        StartFalling();
                    }
                }

                yield return null;
            }
        }
        // ��ź�� ���������� ����� �Լ�
        void StartFalling()
        {
            // ��ź�� Y�� �ӵ��� �߷� ���ӵ��� �߰��Ͽ� �ϰ��ϰ� ����
            rb.useGravity = true;
            rb.velocity += Vector3.down * gravity;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            //base.OnTriggerEnter(other);

        }
    }
