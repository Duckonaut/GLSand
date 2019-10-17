#version 330 core
layout(location = 0) out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
    vec4 color = texture(texture0, texCoord);
	outputColor = vec4(color.r > 0 ? 1.0 : 0.0, color.r > 0 ? 1.0 : 0.0, color.r > 0 ? 1.0 : 0.0, 1.0);
}
