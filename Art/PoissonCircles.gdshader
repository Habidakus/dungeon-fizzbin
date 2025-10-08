shader_type canvas_item;

uniform vec4 background_color : source_color;
uniform vec4 circle_color : source_color;
uniform sampler2D noise_texture;
uniform int size = 20;
uniform float amount : hint_range(0, 1) = 1;

vec2 get_center(vec2 square, vec2 square_uv, vec2 offset)
{
	float sx = abs(texture(noise_texture, square_uv).r);
	float sy = abs(texture(noise_texture, vec2(0.1, 0.1) + square_uv).r);
	return square + offset * vec2(sx, sy);
}

float get_radius(vec2 square, vec2 offset, vec2 our_center, vec2 screen_pixel_size)
{
	float mx = 2.0 * max(offset.x, offset.y);
	for (int dx = -1; dx < 2; ++dx)
	{
		for (int dy = -1; dy < 2; ++dy)
		{
			if (dx == 0 && dy == 0)
				continue;
			vec2 dvec = vec2(float(dx), float(dy));
			vec2 other_square = square + offset * dvec;
			vec2 other_center = get_center(other_square, other_square * screen_pixel_size, offset);
			float d = distance(other_center, our_center);
			mx = min(d, mx);
		}
	}
	
	return mx / 2.0;
}

bool within_circle(vec2 square, vec2 screen_pixel_size, vec2 coord, vec2 offset)
{
	vec2 center_pixel = vec2(0.5,0.5) / screen_pixel_size;
	vec2 center = get_center(square, square * screen_pixel_size, offset);
	float fraction_from_screen_center = distance(center_pixel, center) / distance(vec2(0,0), center_pixel); // 1.0 == point at (0,0), 0.0 == point at screen center
	if (fraction_from_screen_center > amount)
	{
		float our_radius = get_radius(square, offset, center, screen_pixel_size);
		return distance(coord, center) <= our_radius;
	}
	else
	{
		return false;
	}
}

void fragment() {
    vec2 pixel_coords = floor(UV / SCREEN_PIXEL_SIZE);
	int pixel_x = int(pixel_coords.x);
	int pixel_y = int(pixel_coords.y);
	int square_x = pixel_x / size;
	int square_y = pixel_y / size;
	
	vec2 offset = vec2(float(size), float(size));

	bool within = false;
	for (int dx = -1; within == false && dx < 2; ++dx)
	{
		float cx = float(size * (square_x + dx));
		for (int dy = -1; within == false && dy < 2; ++dy)
		{
			float cy = float(size * (square_y + dy));
			vec2 cvec = vec2(cx, cy);
			if (within_circle(cvec, SCREEN_PIXEL_SIZE, pixel_coords, offset))
			{
				within = true;
			}
		}
	}

    if (within) {
        COLOR = circle_color; // Or vec4(1.0, 1.0, 1.0, 1.0); for white
    } else {
        COLOR = background_color; // Transparent black
    }
}