using System;
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
            ps[i].velocity = new Vector3(ps[i].velocity.x, ps[i].velocity.y) + 
                Player.velocity.magnitude * Player.transform.up;
            ps[i].velocity = Vector3.Lerp(ps[i].velocity, Vector3.zero, Time.deltaTime * 2);
            ps[i].position = new Vector3(ps[i].position.x, ps[i].position.y);
        }
        particleSystem.SetParticles(ps, ps.Length);

        Player.OnUnDie += PlayerOnOnUnDie;
        UnDie = false;
    }

    private void PlayerOnOnUnDie()
    {
        UnDie = true;
        var ps = new ParticleSystem.Particle[particleSystem.particleCount];
        particleSystem.GetParticles(ps);

        for (int i = 0; i < ps.Length; i++)
        {
            ps[i].velocity = (Player.transform.position - ps[i].position).normalized*
                             Vector3.Distance(ps[i].position, Player.transform.position);
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
//                ps[i].velocity = new Vector3(ps[i].velocity.x, ps[i].velocity.y) + 
//                    (Player.transform.position - ps[i].position).normalized * Time.deltaTime * 3;
                ps[i].velocity = Vector3.Lerp(ps[i].velocity, Vector3.zero, Time.deltaTime);
//                ps[i].position = new Vector3(ps[i].position.x, ps[i].position.y);
                ps[i].lifetime = 1;
            }
        }
        else
        {
            Die();
            for (int i = 0; i < ps.Length; i++)
            {
//                ps[i].velocity = Vector3.Lerp(ps[i].velocity, Vector3.zero, Time.deltaTime * 2);
//                ps[i].position = new Vector3(ps[i].position.x, ps[i].position.y);
                if (Vector2.Distance(ps[i].position, Player.transform.position) < 1 && ps[i].lifetime > 0.5f)
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
            print("ttttt");
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
