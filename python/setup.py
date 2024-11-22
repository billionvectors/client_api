from setuptools import setup, find_packages

setup(
    name="asimplevectors",
    version="0.1.1",
    description="Python client library for interacting with asimplevectors API.",
    long_description=open("README.md").read(),
    long_description_content_type="text/markdown",
    author="billionvectors",
    author_email="support@billionvectors.com",
    url="https://github.com/billionvectors/client_api.git",
    license="MIT",
    packages=find_packages(),
    install_requires=[
        "httpx>=0.24.1",
        "pydantic>=2.1.1",
        "requests-toolbelt>=0.10.1",
        "aiofiles>=0.8.0",
        "numpy>=1.21.0"
    ],
    classifiers=[
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ],
    python_requires=">=3.8",
    include_package_data=True,
)
