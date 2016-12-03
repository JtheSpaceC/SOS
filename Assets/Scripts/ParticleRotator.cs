using UnityEngine;
using System.Collections;

public class ParticleRotator : MonoBehaviour {

	ParticleSystem m_System;
	ParticleSystem.Particle[] m_Particles;
	ParticleSystem.Burst[] m_Bursts;

	int numParticlesAlive;
	int numBursts;

	Vector2 dir;
	float angle;
	float timeToWait;


	void OnEnable()
	{
		m_System = GetComponent<ParticleSystem>();

		InitializeIfNeeded();

		numBursts = m_System.emission.GetBursts(m_Bursts);

		for (int i = 0; i < numBursts; i++)
		{
			timeToWait = m_Bursts[i].time + m_System.main.startDelay.constant;
			StartCoroutine("DoTheThing");
		}
	}

	IEnumerator DoTheThing()
	{
		yield return new WaitForSeconds(timeToWait);
		yield return new WaitForEndOfFrame();

		InitializeIfNeeded();

		// GetParticles is allocation free because we reuse the m_Particles buffer between updates
		numParticlesAlive = m_System.GetParticles(m_Particles);

		// Change only the particles that are alive
		for (int i = 0; i < numParticlesAlive; i++)
		{
			dir = (m_Particles[i].position).normalized;
			angle = Vector2.Angle(transform.up, dir);
			if(dir.x < 0)
				angle = 360-angle;
			m_Particles[i].rotation = angle;
		}

		// Apply the particle changes to the particle system
		m_System.SetParticles(m_Particles, numParticlesAlive);
	}

	void InitializeIfNeeded()
	{
		if (m_System == null)
			m_System = GetComponent<ParticleSystem>();

		if (m_Particles == null || m_Particles.Length < m_System.maxParticles)
			m_Particles = new ParticleSystem.Particle[m_System.maxParticles]; 

		if (m_Bursts == null)
			m_Bursts = new ParticleSystem.Burst[m_System.emission.burstCount];
	}

}
