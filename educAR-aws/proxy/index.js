const imageType = require('image-type');
const fetch = require('node-fetch');

exports.handler = async (event) => {
  const url = event.queryStringParameters.url;

  try {
    const image = await fetchImage(url);
    const type = imageType(image);
    console.log(type);
    return {
      statusCode: 200,
      headers: {
          'Content-Type': type.mime,
          'Access-Control-Allow-Methods': 'GET',
          'Access-Control-Allow-Origin': '*'
      },
      body: image.toString('base64'),
      isBase64Encoded: true
    };
  } catch (error) {
      console.log(error);
      return {
          statusCode: 500,
          body: error
      };
  }
};

function fetchImage(url) {
  return fetch(url).then(res => res.buffer());
}
