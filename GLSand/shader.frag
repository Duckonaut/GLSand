#version 330 core
layout(location = 0) out vec4 outputColor;

in vec2 texCoord;
uniform vec2 cpos;

uniform sampler2D texture0;

bool close(vec2 a, vec2 b) 
{
	return abs(a.x - b.x) < 2.0 && abs(a.y - b.y) < 2.0;
}

void main()
{
	if(close(texCoord * 512, cpos))
		outputColor = vec4(1.0/256.0, 1.0, 1.0, 1.0);
    else outputColor = texture(texture0, texCoord);
	
}
