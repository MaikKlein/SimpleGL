#version 400
 
in vec3 vPosition;
in vec3 vNormal;

out vec4 eyeCordFs;
out vec4 eyeNormalFs;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{   
    mat4 modelView = view * model;
    mat4 normalMatrix = transpose(inverse(view * model));
    vec4 eyeNorm = normalize(normalMatrix * vec4(vNormal, 0.0));
    vec4 eyeCord = modelView * vec4(vPosition, 1.0);

    eyeCordFs = eyeCord;
    eyeNormalFs = eyeNorm;

    gl_Position = proj * modelView * vec4( vPosition,1.0);
}
