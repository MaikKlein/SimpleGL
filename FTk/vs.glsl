#version 400
 
in vec3 vPosition;
in vec3 vColor;
out vec4 color;
uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
void main()
{   
    mat4 modelview = view * model;
    mat4 normal_matrix = transpose(inverse(modelview));
    gl_Position =  proj * modelview * vec4(vPosition, 1.0);
    color = vec4( vColor, 1.0);
}
