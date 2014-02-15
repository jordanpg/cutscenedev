$CutsceneModuleServer::Datablocks = true;

//DATABLOCKS
datablock fxDTSBrickData(BrickCameraData)
{
	brickFile = $Cutscene::Root @ "data/cam.blb";
	iconName = $Cutscene::Root @ "data/cam";

	category = "Special";
	subCategory = "Director";
	uiName = "Camera";
};

datablock StaticShapeData(CameraShapeData)
{
	shapeFile = $Cutscene::Root @ "data/camera.dts";
};

datablock StaticShapeData(CameraShapeGoldData)
{
	shapeFile = $Cutscene::Root @ "data/camera.dts";

	doColorShift = true;
	colorShiftColor = "1 1 0 1";
};


datablock PathCameraData(CutsceneCamera)
{
	doritos = true;
};

//CAMERA BOLLOCKS
datablock ParticleData(CameraSelectionParticle)
{
	textureName = "base/data/particles/star1";

	dragCoefficient = 3;
	gravityCoefficient = 0.2;

	inheritedVelFactor = 0.15;
	constantAcceleration = 0.0;

	lifetimeMS = 750;
	lifetimeVarianceMS = 150;

	spinSpeed = 10.0;
	spinRandomMin = -150.0;
	spinRandomMax = 150.0;

	colors[0] = "1.0 1.0 1.0 0.6";
	colors[1] = "0.8 0.8 0.8 0.0";

	sizes[0] = 0.5;
	sizes[1] = 0.2;

	times[0] = 0.0;
	times[1] = 1.0;

	useInvAlpha = false;
};

datablock ParticleEmitterData(CameraSelectionEmitter)
{
	particles = "CameraSelectionParticle";

	lifetimeMS				= 0;
	ejectionPeriodMS		= 10;
	periodVarianceMS		= 0;
	ejectionVelocity		= 5;
	thetaMin 				= 0;
	thetaMax 				= 180;
	phiReferenceVel 		= 0;
	phiVariance 			= 360;
	overrideAdvance 		= false;
	useEmitterColors 		= false;
	orientParticles 		= false;

	uiName = "Camera Selection";
};

datablock ExplosionData(CameraSelectExplosion)
{
	lifetimeMS = 500;
	delayMS = 200;

	emitter[0] = CameraSelectionEmitter;

	shakeCamera = false;

	explosionScale = "1 1 1";


};

datablock ParticleData(CameraCreationParticle)
{
	textureName = "base/data/particles/star1";

	dragCoefficient = 3;
	gravityCoefficient = 0.0;

	inheritedVelFactor = 0.15;
	constantAcceleration = 0.0;

	lifetimeMS = 1000;
	lifetimeVarianceMS = 150;

	spinSpeed = 10.0;
	spinRandomMin = -150.0;
	spinRandomMax = 150.0;

	colors[0] = "0.25 0.25 0.25 0.2";
	colors[1] = "0.5 0.5 0.5 0.4";
	colors[2] = "0.8 0.8 0.8 0.6";

	sizes[0] = 0.2;
	sizes[1] = 0.4;
	sizes[2] = 0.5;

	times[0] = 0.0;
	times[1] = 0.5;
	times[2] = 1.0;

	useInvAlpha = false;
};

datablock ParticleEmitterData(CameraCreationEmitter)
{
	particles = "CameraCreationParticle";

	lifetimeMS				= 0;
	ejectionPeriodMS		= 10;
	periodVarianceMS		= 0;
	ejectionVelocity		= -5;
	ejectionOffset 			= 2;
	thetaMin 				= 0;
	thetaMax 				= 180;
	phiReferenceVel 		= 0;
	phiVariance 			= 360;
	overrideAdvance 		= false;
	useEmitterColors 		= false;
	orientParticles 		= false;

	uiName = "Camera Creation";
};

datablock ExplosionData(CameraCreationExplosion)
{
	lifetimeMS = 500;
	delayMS = 200;

	emitter[0] = CameraCreationEmitter;

	shakeCamera = false;

	explosionScale = "1 1 1";
};

datablock ParticleData(CameraDestroyParticle)
{
	textureName = "base/data/particles/star1";

	dragCoefficient = 3;
	gravityCoefficient = 0.0;

	inheritedVelFactor = 0.15;
	constantAcceleration = 0.0;

	lifetimeMS = 1500;
	lifetimeVarianceMS = 150;

	spinSpeed = 50.0;
	spinRandomMin = -150.0;
	spinRandomMax = 150.0;

	colors[0] = "0.8 0.8 0.8 0.6";
	colors[1] = "0.5 0.5 0.5 0.2";
	colors[2] = "0.25 0.25 0.25 0.0";

	sizes[0] = 0.5;
	sizes[1] = 0.4;
	sizes[2] = 0.2;

	times[0] = 0.0;
	times[1] = 0.5;
	times[2] = 1.0;

	useInvAlpha = false;
};

datablock ParticleEmitterData(CameraDestroyEmitter)
{
	particles = "CameraDestroyParticle";

	lifetimeMS				= 0;
	ejectionPeriodMS		= 10;
	periodVarianceMS		= 0;
	ejectionVelocity		= 5;
	ejectionOffset 			= 0.25;
	thetaMin 				= 0;
	thetaMax 				= 180;
	phiReferenceVel 		= 0;
	phiVariance 			= 360;
	overrideAdvance 		= false;
	useEmitterColors 		= false;
	orientParticles 		= false;

	uiName = "Camera Destroy";
};

datablock ExplosionData(CameraDestroyExplosion)
{
	lifetimeMS = 500;
	delayMS = 200;

	emitter[0] = CameraDestroyEmitter;

	shakeCamera = false;

	explosionScale = "1 1 1";
};

datablock ProjectileData(CameraSelectProjectile)
{
	lifetime = 0;
	projectileShapeName = "base/data/shapes/empty.dts";
	directDamage = 0;
	explosion = "CameraSelectExplosion";
	explodeOnDeath = true;
	hasLight = false;
	isBallistic = false;
	muzzleVelocity = 0;
};

datablock ProjectileData(CameraCreationProjectile)
{
	lifetime = 0;
	projectileShapeName = "base/data/shapes/empty.dts";
	directDamage = 0;
	explosion = "CameraCreationExplosion";
	explodeOnDeath = true;
	hasLight = false;
	isBallistic = false;
	muzzleVelocity = 0;
};

datablock ProjectileData(CameraDestroyProjectile)
{
	lifetime = 0;
	projectileShapeName = "base/data/shapes/empty.dts";
	directDamage = 0;
	explosion = "CameraDestroyExplosion";
	explodeOnDeath = true;
	hasLight = false;
	isBallistic = false;
	muzzleVelocity = 0;
};