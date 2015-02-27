using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{

    public PlayerController Player;
    private bool UnDie;

    // Use this for initialization
    void Start()
    {
        particleSystem.startColor = Player.Player.GetColor();
        particleSystem.Emit(particleSystem.maxParticles);
        var ps = new ParticleSystem.Particle[particleSystem.particleCount];
        particleSystem.GetParticles(ps);
        for (int i = 0; i < ps.Length; i++)
        {
            ps[i].velocity = new Vector3(ps[i].velocity.x, ps[i].velocity.y);
            ps[i].velocity = Vector3.Lerp(ps[i].velocity, Vector3.zero, Time.deltaTime * 2);
            ps[i].position = new Vector3(ps[i].position.x, ps[i].position.y, 0.5f);
        }
        particleSystem.SetParticles(ps, ps.Length);

        Player.OnUnDie += PlayerOnOnUnDie;
        UnDie = false;
        transform.position += Vector3.back*0.5f;
    }

    private void PlayerOnOnUnDie()
    {
        UnDie = true;
        var ps = new ParticleSystem.Particle[particleSystem.particleCount];
        particleSystem.GetParticles(ps);

        for (int i = 0; i < ps.Length; i++)
        {
            ps[i].velocity = -ps[i].position;
        }

        particleSystem.SetParticles(ps, ps.Length);
    }

    // Update is called once per frame
    void Update()
    {
        var ps = new ParticleSystem.Particle[particleSystem.particleCount];
        particleSystem.GetParticles(ps);

        if (!UnDie)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                var distortion = Random.onUnitSphere;
                distortion.Scale(new Vector3(1, 1, 0));
                ps[i].velocity += distortion; 
                ps[i].velocity = Vector3.Lerp(ps[i].velocity, Vector3.zero, Time.deltaTime);
                ps[i].lifetime = 1;
            }
        }
        else
        {
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].position.magnitude < 7 && ps[i].lifetime > 0.5f)
                {
                    ps[i].lifetime = 0;
                    ps[i].velocity = Vector3.zero;
                    ps[i].position = Player.transform.position;
                }
                else
                {
                    ps[i].lifetime = 1;
                }
            }
        }
        particleSystem.SetParticles(ps, ps.Length);

        if (ps.Length == 0 && UnDie)
        {
            Die();
        }
    }

    private void Die()
    {
        Player.OnUnDie -= PlayerOnOnUnDie;
        Destroy(this);
        Player.UnDie();
    }
}
