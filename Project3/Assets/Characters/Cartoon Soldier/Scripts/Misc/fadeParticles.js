var particleFade : AnimationCurve;
var color : Color;
var minSize : float;
var maxSize : float;

function Update () {
	var particles : Particle[] = GetComponent.<ParticleEmitter>().particles;
	for (var i = 0; i < particles.Length; i++){
		particles[i].color = color;
		particles[i].color.a = particleFade.Evaluate(1-(particles[i].energy / particles[i].startEnergy));
		var energyVariation : float = GetComponent.<ParticleEmitter>().maxEnergy - GetComponent.<ParticleEmitter>().minEnergy;
		var particleEnergyVariation : float = particles[i].startEnergy - GetComponent.<ParticleEmitter>().minEnergy;
		var makeSize : float = particleEnergyVariation / energyVariation;
		particles[i].size = Mathf.Lerp(minSize, maxSize, makeSize);
	}
	GetComponent.<ParticleEmitter>().particles = particles;
	
}