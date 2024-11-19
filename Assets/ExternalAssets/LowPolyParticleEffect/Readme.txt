Custom shader: ParticleSurface, ParticleSurfaceTransparent, Particle Unlit

Some particle materials are using these custom shaders for fake shading effects without scene lighting or controlable intensity. ParticleSurface, ParticleSurfaceTransparent would be affected by vertex world normal and view direction, but not scene lighting.

Some particle materials are using URP particles lit.

Some platforms do not support mesh GPU instancing so mesh particles using these built-in particle shaders are not rendered. In this case, you can disable it in 'Particle System' - 'Renderer' - 'Enable Mesh GPU Instancing'.