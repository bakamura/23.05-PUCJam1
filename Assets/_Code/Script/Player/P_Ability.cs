using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class P_Ability : Singleton<P_Ability> {

    [Header("Heal")]

    [SerializeField] private int _healPower;
    public int HealPower { get { return _healPower; } }
    [SerializeField] private float _healDelay;
    [SerializeField] private GameObject _healEffect;

    [Space(16)]

    [SerializeField] private Vector2 _healEffectSpawnOffset;
    public Vector2 HealEffectSpawnOffset { get { return _healEffectSpawnOffset; } }
    [SerializeField] private float _healEffectMotion;
    public float HeadEffectMotion { get { return _healEffectMotion; } }
    [SerializeField] private float _healEffectDuration;
    public float HealEffectDuration { get { return _healEffectDuration; } }

    [Header("Dodge")]

    [SerializeField] private float _dodgeDistance;
    [SerializeField] private float _dodgeIFrameDuration;

    [Header("Interact")]

    [SerializeField] private float _interactRange;
    [SerializeField] private LayerMask _interactLayer;
    private Interactable _nearestInteractable;


    [Header("Events")]

    private UnityEvent _onHeal = new UnityEvent();
    public UnityEvent OnHeal { get { return _onHeal; } }
    private UnityEvent _onDodge = new UnityEvent();
    public UnityEvent OnDodge { get { return _onDodge; } }
    private UnityEvent _onInteract = new UnityEvent();
    public UnityEvent OnInteract { get { return _onInteract; } }
    private UnityEvent _onEnterDoor = new UnityEvent();
    public UnityEvent OnEnterDoor { get { return _onInteract; } }

    [Header("Cache")]

    private SpriteRenderer _healEffectRenderer;
    public SpriteRenderer HealEffectRenderer { get { return _healEffectRenderer; } }
    private float _nearestInteractableDistance;
    private float _cacheInteractableDistance;
    private WaitForSeconds _healWait;
    private WaitForSeconds _healRecoilWait;
    private WaitForSeconds _dodgeIFrameWait;
    private WaitForSeconds _dodgeRecoilWait;

    private void Start() {
        GameObject go = Instantiate(_healEffect); // Pool of only one
        _healEffect = go; 
        _healEffect.SetActive(false);
        _healEffectRenderer = _healEffect.GetComponent<SpriteRenderer>();

        _healWait = new WaitForSeconds(_healDelay);
        _healRecoilWait = new WaitForSeconds(P_Animation.Instance.GetAnimationDuration(P_Animation.P_HEAL) - _healDelay);
        _dodgeIFrameWait = new WaitForSeconds(_dodgeIFrameDuration);
        _dodgeRecoilWait = new WaitForSeconds(P_Animation.Instance.GetAnimationDuration(P_Animation.P_DODGE) - _dodgeIFrameDuration);
    }

    private void Update() {
        if (P_Movement.Instance.enabled && P_Movement.Instance.PlayerGrounded()) {
            if (InputHandler.Instance.Heal) StartCoroutine(Heal());
            if (InputHandler.Instance.Dodge) StartCoroutine(Dodge());
            if (InputHandler.Instance.Interact) QueryInteract();
        }
    }

    private IEnumerator Heal() {
        _onHeal?.Invoke();
        P_Movement.Instance.RigidBody2D.velocity = Vector2.zero;
        P_Movement.Instance.enabled = false;

        yield return _healWait;

        _healEffectRenderer.flipX = P_Animation.Instance.SpriteRenderer.flipX;
        _healEffect.SetActive(true);

        yield return _healRecoilWait;

        P_Movement.Instance.enabled = true; // Check if game is paused
    }

    private IEnumerator Dodge() {
        _onDodge?.Invoke();
        P_Movement.Instance.enabled = false;
        P_Movement.Instance.RigidBody2D.velocity = -Physics2D.gravity.y * (_dodgeIFrameDuration / _dodgeDistance) * ((InputHandler.Instance.Movement != 0 ? InputHandler.Instance.Movement < 0 : P_Animation.Instance.SpriteRenderer.flipX) ? Vector2.left : Vector2.right);

        yield return _dodgeIFrameWait;

        // Stuff
        P_Movement.Instance.RigidBody2D.velocity = Vector2.zero;

        yield return _dodgeRecoilWait;

        P_Movement.Instance.enabled = true;
    }

    private void QueryInteract() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _interactRange, _interactLayer);
        for (int i = 0; i < hits.Length; i++) {
            if (i == 0) {
                _nearestInteractable = hits[i].GetComponent<Interactable>();
                _nearestInteractableDistance = Vector3.Distance(transform.position, _nearestInteractable.transform.position);
            }
            else {
                _cacheInteractableDistance = Vector3.Distance(transform.position, hits[0].transform.position);
                if (_cacheInteractableDistance < _nearestInteractableDistance) {
                    _nearestInteractable = hits[0].GetComponent<Interactable>();
                    _nearestInteractableDistance = Vector3.Distance(transform.position, _nearestInteractable.transform.position);
                }
            }
        }
        //_nearestInteractable.
    }

    private IEnumerator Interact() {
        _onInteract?.Invoke();
        P_Movement.Instance.enabled = false;

        yield return null;
    }

    private IEnumerator EnterDoor() {
        _onEnterDoor?.Invoke();
        P_Movement.Instance.enabled = false;

        yield return null;
    }
}