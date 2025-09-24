#!/bin/bash
# Convert JSON to environment variables
#!/bin/bash

# Usage: ./json-to-env.sh config.json output.env

INPUT_JSON="/Users/bad_apple/.microsoft/usersecrets/a511a789-990a-448d-bcae-e354f4bf908c/secrets.json"
OUTPUT_ENV="../credentials/.env"

if [ -z "$INPUT_JSON" ] || [ -z "$OUTPUT_ENV" ]; then
    echo "Usage: $0 <input.json> <output.env>"
    exit 1
fi

if [ ! -f "$INPUT_JSON" ]; then
    echo "Error: File $INPUT_JSON not found."
    exit 1
fi

# Process JSON and write to environment file
jq -r '
  def sanitize_key(k): (k | gsub(":";"__"));
  def flatten(prefix):
    to_entries | map(
      if (.value | type == "object") then
        .value | flatten(prefix + sanitize_key(.key) + "__")
      else
        prefix + sanitize_key(.key) + "=\(.value | @json)"
      end
    ) | .[];
  . | flatten("")
' "$INPUT_JSON" | while IFS= read -r line; do
    key="${line%%=*}"
    raw_value="${line#*=}"
    # Unquote JSON strings safely
    value=$(jq -r <<< "$raw_value")
    echo "Environment=${key}=${value}"
done > "$OUTPUT_ENV"


echo "✅ Created: $OUTPUT_ENV"
