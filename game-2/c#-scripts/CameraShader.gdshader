shader_type canvas_item;

uniform sampler2D camera_y;
uniform sampler2D camera_CbCr;

void fragment() {
	vec3 ycbcr;
	ycbcr.x = texture(camera_y, UV).r;
	ycbcr.yz = texture(camera_CbCr, UV).rg;

	float y = ycbcr.x;
	float cb = ycbcr.y - 0.5;
	float cr = ycbcr.z - 0.5;

	vec3 rgb;
	rgb.r = y + 1.402 * cr;
	rgb.g = y - 0.344136 * cb - 0.714136 * cr;
	rgb.b = y + 1.772 * cb;

	COLOR = vec4(rgb, 1.0);
}
