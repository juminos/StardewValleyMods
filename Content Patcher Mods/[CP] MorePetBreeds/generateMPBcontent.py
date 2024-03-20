import json
import os
import re

# Function to extract the numerical part from a filename
def extract_number(filename):
    match = re.search(r'\d+', filename)
    return int(match.group()) if match else None

# Function to get the filenames of PNG files in a directory excluding those with "icon" in their name
def get_filenames(directory, prefix):
    return [file for file in os.listdir(directory) if file.lower().endswith(".png") and prefix in file.lower()]

# Directories for Cats and Dogs
cats_directory = "assets/Cat"
dogs_directory = "assets/Dog"

# Get the filenames for Cats and Dogs
cat_filenames = [os.path.splitext(filename)[0] for filename in get_filenames(cats_directory, "cat")]
dog_filenames = [os.path.splitext(filename)[0] for filename in get_filenames(dogs_directory, "dog")]

# Determine the number of Cats and Dogs based on the filenames
num_cats = len(cat_filenames)
num_dogs = len(dog_filenames)

# Base JSON structure for one iteration
base_json = {
    "Format": "1.29.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Pets",
            "TargetField": ["Cat", "Breeds"],
            "Entries": {}
        },
        {
            "Action": "EditData",
            "Target": "Data/Pets",
            "TargetField": ["Dog", "Breeds"],
            "Entries": {}
        },
        {
            "Action": "Load",
            "Target": "",
            "FromFile": "assets/Cat/{{TargetWithoutPath}}.png"
        },
        {
            "Action": "Load",
            "Target": "",
            "FromFile": "assets/Dog/{{TargetWithoutPath}}.png"
        }
    ]
}

# Iterate to generate entries for Cats
for i, cat_filename in enumerate(cat_filenames, start=1):
    entry_key = f"juminos.MorePetBreeds_Cat_{i}"
    
    entry_value = {
        "ID": entry_key,
        "Texture": f"Mods/juminos.MorePetBreeds/{cat_filename}",
        "IconTexture": f"Mods/juminos.MorePetBreeds/{cat_filename}",
        "IconSourceRect": {
            "X": 73,
            "Y": 135,
            "Width": 16,
            "Height": 16
        },
        "BarkOverride": None,
        "VoicePitch": 1.0
    }
    base_json["Changes"][0]["Entries"][entry_key] = entry_value

    # Update "Load" action Target for Cat_i
    base_json["Changes"][2]["Target"] += f"Mods/juminos.MorePetBreeds/{cat_filename}, "

# Remove the trailing comma and space from the "Load" action Target for Cats
base_json["Changes"][2]["Target"] = base_json["Changes"][2]["Target"].rstrip(", ")

# Iterate to generate entries for Dogs
for i, dog_filename in enumerate(dog_filenames, start=1):
    entry_key = f"juminos.MorePetBreeds_Dog_{i}"
    
    entry_value = {
        "ID": entry_key,
        "Texture": f"Mods/juminos.MorePetBreeds/{dog_filename}",
        "IconTexture": f"Mods/juminos.MorePetBreeds/{dog_filename}",
        "IconSourceRect": {
            "X": 73,
            "Y": 132,
            "Width": 16,
            "Height": 16
        },
        "BarkOverride": None,
        "VoicePitch": 1.0
    }
    base_json["Changes"][1]["Entries"][entry_key] = entry_value

    # Update "Load" action Target for Dog_i
    base_json["Changes"][3]["Target"] += f"Mods/juminos.MorePetBreeds/{dog_filename}, "


# Remove the trailing comma and space from the "Load" action Target for Dogs
base_json["Changes"][3]["Target"] = base_json["Changes"][3]["Target"].rstrip(", ")

# Save the result to a JSON file
with open("content.json", "w") as json_file:
    json.dump(base_json, json_file, indent=2)
