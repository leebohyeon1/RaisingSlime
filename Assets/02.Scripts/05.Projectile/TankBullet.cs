    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TankBullet : Bullet
    {

        public float gravity = 9.81f; // 하강할 때 적용될 가속도
        public float fallDistance = 3f; // 플레이어와의 거리가 이 값보다 작아지면 포탄이 떨어지기 시작
    
        private Vector3 targetPos = Vector3.zero; // 타겟이 되는 플레이어 또는 목적지
        private bool isFalling = false;

        // Start is called before the first frame update
        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            lifeTimer = lifetime; // 수명 타이머 초기화
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

                // 플레이어와의 거리가 설정한 fallDistance보다 작아지면 하강 시작
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

        // 일정 거리 이상 플레이어에게 가까워지면 포탄이 떨어지기 시작하는 코루틴
        IEnumerator CheckFallDistance()
        {
            while (!isFalling)
            {
                if (targetPos != Vector3.zero)
                {
                    float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x,0, transform.position.z), new Vector3(targetPos.x,0, targetPos.z));

   

                    // 플레이어와의 거리가 설정한 fallDistance보다 작아지면 하강 시작
                    if (distanceToPlayer <= fallDistance)
                    {
                        isFalling = true;
                        StartFalling();
                    }
                }

                yield return null;
            }
        }
        // 포탄이 떨어지도록 만드는 함수
        void StartFalling()
        {
            // 포탄의 Y축 속도에 중력 가속도를 추가하여 하강하게 만듦
            rb.useGravity = true;
            rb.velocity += Vector3.down * gravity;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            //base.OnTriggerEnter(other);

        }
    }
