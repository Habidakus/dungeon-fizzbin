shader_type canvas_item;

uniform sampler2D noise_texture;
uniform vec2 noise_offset;
uniform int spacing = 11;
uniform int width = 2;
uniform float order = 3.0f;

// shader_parameter/noise_texture noise offset
void fragment() {
    vec2 pixel_coords = noise_offset / 15.0 + vec2(0.25,0.25) + (UV / 2.0); // SCREEN_PIXEL_SIZE;
	float n = abs(texture(noise_texture, pixel_coords).r);
	vec2 offset = UV - vec2(0.5, 0.5);
	float dist_squared = sqrt(offset.x * offset.x + offset.y * offset.y);
	float mod_dist = dist_squared + n / order;
	int int_dist = int(floor(mod_dist * 490.0));
	int mod_eleven = int_dist % spacing;
	if (mod_eleven + width > spacing)
		COLOR.rg = vec2(1,1) - COLOR.rg;
}
