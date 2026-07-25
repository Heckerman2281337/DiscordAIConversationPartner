FROM pytorch/pytorch:2.5.1-cuda12.1-cudnn9-runtime
WORKDIR /app


RUN DEBIAN_FRONTEND=noninteractive apt-get update && \
	DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends\
	ffmpeg \
	build-essential \
	tzdata \
	python3-dev\
	portaudio19-dev\
	libsndfile1\
	&& rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

CMD ["python", "-m", "xtts_api_server", "--host", "0.0.0.0", "--port", "8020"]