build:
	docker build -t imranq2/fhir-converter .

run:
	docker run --rm -it --mount type=bind,source="${PWD}"/data/SampleData,target=/SampleData --mount type=bind,source="${PWD}"/data/Out,target=/Out imranq2/fhir-converter convert -d /Templates/Ccda -r CCD -i /SampleData -o /Out
