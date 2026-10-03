shader_type canvas_item;

/** a noise texture to make things nicely random */
uniform sampler2D noise_texture;
/** number of rings on the texture at the middle part */
uniform float rings = 7;
/** the higher this number, the thinner the track that the lock shuttles travel on */
uniform int bands = 29;
/** the higher this number, the wider the width of the shuttles */
uniform int band_expansion = 5;
/** The higher this number, the less shuttles will appear on the tracks */
uniform float band_shortness = 3.5;
/** The higher this number, the faster average speed of the shuttles */
uniform float turn_speed = 0.05;
/** the higher this number, the more erratic the shuttles jitter will be (and zero will have them perform no jitter) */
uniform float jitter_speed = 1.0;
/** the color of the shuttles */ 
uniform vec4 paint_color : source_color;

void fragment() {
	float d = distance(UV, vec2(0.5, 0.5));
	float f_ring = d * 2.0 * rings;
	float angle = atan(UV.y - 0.5, UV.x - 0.5);
	float angle_delta = turn_speed * TIME;
	int ring = int(ceil(f_ring));
	angle_delta /= float(ring);
	float mid_ring_frac = float(ring) - f_ring;
	int index = int(mid_ring_frac * float(bands));
	float jitter_angle = angle_delta;
	vec2 jitter_loc_1 = float(ring) * vec2(cos(jitter_angle), sin(jitter_angle)) / (5.0 * rings) + vec2(0.5, 0.5);
	jitter_angle += TAU / 3.0;
	vec2 jitter_loc_2 = float(ring) * vec2(cos(jitter_angle), sin(jitter_angle)) / (5.0 * rings) + vec2(0.5, 0.5);
	jitter_angle += TAU / 3.0;
	vec2 jitter_loc_3 = float(ring) * vec2(cos(jitter_angle), sin(jitter_angle)) / (5.0 * rings) + vec2(0.5, 0.5);
	float jitter = texture(noise_texture, jitter_loc_1).r + texture(noise_texture, jitter_loc_2).r + texture(noise_texture, jitter_loc_3).r;
	if (ring % 2 == 1)
		angle += angle_delta + jitter * jitter_speed;
	else
		angle -= angle_delta + jitter * jitter_speed;
	vec2 f_width_loc = float(ring) * vec2(cos(angle), sin(angle)) / (4.0 * rings) + vec2(0.5, 0.5);
	float f_width = texture(noise_texture, f_width_loc).r;
	int min_band = int(bands/2);
	int max_band = min_band;
	if (f_width * band_shortness < 1.0)
	  min_band -= band_expansion;
	else if (f_width * band_shortness > (band_shortness - 1.0))
	  max_band += band_expansion;
	if (index >= min_band && index <= max_band)
		COLOR = paint_color;
}
