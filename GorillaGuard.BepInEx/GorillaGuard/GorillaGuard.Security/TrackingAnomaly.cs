using System;

namespace GorillaGuard.Security;

[Flags]
internal enum TrackingAnomaly
{
	None = 0,
	InvalidSample = 1,
	LeftReach = 2,
	RightReach = 4,
	HeadJump = 8,
	LeftHandJump = 0x10,
	RightHandJump = 0x20,
	HeadSpeed = 0x40,
	LeftHandSpeed = 0x80,
	RightHandSpeed = 0x100,
	HeadAcceleration = 0x200,
	LeftHandAcceleration = 0x400,
	RightHandAcceleration = 0x800
}
