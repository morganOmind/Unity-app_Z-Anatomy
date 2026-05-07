import csv
import os
import shutil

nav_id_file = 'cat_navid.csv'
input_description_files_folder = 'input_cat_descriptions'
output_description_files_folder = 'output_cat_descriptions'

if not os.path.exists(output_description_files_folder):
    os.mkdir(output_description_files_folder)

navids = []
names = []
    
with open(nav_id_file, mode='r', encoding='utf-8') as csv_file:
    csv_reader = csv.DictReader(csv_file, delimiter=';')
    line_count = 0
    for row in csv_reader:
        if line_count == 0:
            print(f'Column names are {", ".join(row)}')
            
        navids.append(row['NAVID'])
        names.append(row['Français'])

        line_count += 1
        
    print(f'Processed {line_count} lines.')
    
for file in os.listdir(input_description_files_folder):
    navid = file.replace(".txt", "").replace("_", "*")
    if not navid in navids:
        print(f'Navid {navid} not found')
    else:
        shutil.copyfile(input_description_files_folder + '/' + file, output_description_files_folder + '/' + names[navids.index(navid)] + '.txt')