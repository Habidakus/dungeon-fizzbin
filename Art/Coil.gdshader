shader_type canvas_item;

/** A 2d noise that will define how the rings mutate */ 
uniform sampler2D noise_texture;
/** A 2d noise that will define how the rings mutate */ 
uniform sampler2D width_texture;
/** the higher this value, the faster the rings will mutate */
uniform float speed = 0.033;
uniform float rotate_speed = 0.016;
/** dictates the empty space between the rings */ 
uniform int spacing = 11;
/** how wide the rings will be */
uniform int width = 2;
/** The higher this value the more orderly the rings will be */
uniform float order = 3.0f;
uniform float extra_width_threshold = 0.5f;
uniform float extra_width_max = 3.0f;
const float zoom = 490.0;

// shader_parameter/noise_texture noise offset
void fragment() {
	
	float bsm_angle = speed * TIME;
	float rotate_angle = rotate_speed * TIME;
	vec2 noise_offset = vec2(cos(bsm_angle), sin(bsm_angle));
    vec2 pixel_coords = noise_offset / 15.0 + vec2(0.25,0.25) + (UV / 2.0); // SCREEN_PIXEL_SIZE;
	float n = abs(texture(noise_texture, pixel_coords).r);
	//float wdelta = abs(texture(width_texture, UV).r);
	vec2 offset = UV - vec2(0.5, 0.5);
	float dist_squared = sqrt(offset.x * offset.x + offset.y * offset.y);
	int ring = int(dist_squared * zoom / float(spacing));
	float radius_squared = float(spacing * ring) / zoom;
	float current_angle = atan(UV.y - 0.5, UV.x - 0.5);
	float angle_dir = float((ring % 2) * 2 - 1);
	vec2 width_offset = vec2(cos(current_angle + rotate_angle * angle_dir), sin(current_angle + rotate_angle * angle_dir));
	vec2 width_coord = vec2(0.5, 0.5) + width_offset * sqrt(radius_squared);
	float wdelta = abs(texture(width_texture, width_coord).r);
	float mod_dist = dist_squared + n / order;
	int int_dist = int(floor(mod_dist * zoom));
	int mod_eleven = int_dist % spacing;
	int mwidth = width;
	if (wdelta > extra_width_threshold)
	{
		mwidth += int(extra_width_max);
	//	mwidth += int(extra_width_max * (wdelta - extra_width_threshold) / (1.0f - extra_width_threshold));
	}
	if (mod_eleven + mwidth > spacing)
		COLOR.rg = (vec2(1,1) - COLOR.rg) / 2.0;
}
