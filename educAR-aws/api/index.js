const AWS = require("aws-sdk");

const dynamo = new AWS.DynamoDB.DocumentClient();
const bcrypt = require("bcryptjs");

const table_name = "channels";

exports.handler = async (event, context) => {
  let body;
  let statusCode = 200;
  const headers = {
    "Content-Type": "application/json",
    "Access-Control-Allow-Headers" : "Content-Type",
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "OPTIONS, POST, GET, PUT, DELETE"
  };

  function reformat_channel_json(channel_json) {
    let scenes_list = [];
    let scenes_dict = channel_json.scenes;
    for (let key in scenes_dict) {
      let scene = scenes_dict[key];
      scene["name"] = key;
      scenes_list.push(scene);
    }
    channel_json.scenes = scenes_list;
    return channel_json;
  }

  async function delete_channel(event) {
    try {
      await dynamo
        .delete({
          TableName: table_name,
          Key: {
            id: event.pathParameters["channel_id"]
          },
          ConditionExpression: "id_teacher = :id_teacher",
          ExpressionAttributeValues: {
            ":id_teacher": event.requestContext.authorizer["id-teacher"]
          }
        })
        .promise();
      statusCode = 204;
    } catch (e) {
      statusCode = 400;
      body = {
        message: e.message
      };
    }
  }

  async function get_channel(event, requestBody) {
    let result = await dynamo
      .get({
        TableName: table_name,
        Key: {
          id: event.pathParameters["channel_id"]
        }
      })
      .promise();
      
    if (!Object.keys(result).length) {
      statusCode = 404;
      throw new Error("Canal nao encontrado");
    } else {
      result = result.Item;
    }
      
    if (result.password == null) {
      body = reformat_channel_json(result);
    } else {
      if (requestBody != null && requestBody.password != null && bcrypt.compareSync(requestBody.password, result.password)) {
        delete result["password"];
        body = reformat_channel_json(result);
      } else {
        statusCode = 401;
        throw new Error("Senha incorreta");
      }
    }
  }

  async function add_channel(event, requestBody) {
    if (requestBody == null) {
      throw new Error("Canal vazio");
    }

    requestBody["id_teacher"] = event.requestContext.authorizer["id-teacher"];
    statusCode = 201;
    
    if (requestBody.password != null && requestBody.password != "") {
      var salt = bcrypt.genSaltSync(10);
      var hash = bcrypt.hashSync(requestBody.password, salt);

      requestBody.password = hash;
    }

    const params = {
      "TableName": table_name,
      "Item": requestBody,
      ConditionExpression: "attribute_not_exists(id)"
    }
    try {
      await dynamo.put(params).promise();
    } catch (e) {
      statusCode = 400;
      body = {
        message: e.message
      };
    }
  }

  async function update_channel(event, requestBody) {
    const params = {
        TableName: table_name,
        Key: {
          id: event.pathParameters["channel_id"]
        },
        ConditionExpression: "id_teacher = :id_teacher",
        UpdateExpression: "set #s = :scenes",
        ExpressionAttributeNames: {
          "#s": "scenes"
        },
        ExpressionAttributeValues: {
          ":scenes": requestBody.scenes,
          ":id_teacher": event.requestContext.authorizer["id-teacher"]
        }
    };
    if (requestBody.password != null && requestBody.password != "") {
      var salt = bcrypt.genSaltSync(10);
      var hash = bcrypt.hashSync(requestBody.password, salt);

      params.UpdateExpression = "set #s = :scenes, #p = :password";
      params.ExpressionAttributeNames = {
        "#s": "scenes",
        "#p": "password"
      };
      params.ExpressionAttributeValues = {
        ":scenes": requestBody.scenes,
        ":password": hash,
        ":id_teacher": event.requestContext.authorizer["id-teacher"]
      };
    }
    try {
      await dynamo.update(params).promise();
      statusCode = 204;
      body = {};
    } catch (err) {
      statusCode = 400;
      throw new Error("Erro ao atualizar canal");
    }
  }

  async function update_scene(event, requestBody) {
    const params = {
        TableName: table_name,
        Key: {
          id: event.pathParameters["channel_id"]
        },
        ConditionExpression: "id_teacher = :id_teacher",
        UpdateExpression: "set #s.#n = :s",
        ExpressionAttributeNames: {
          "#s": "scenes",
          "#n": event.pathParameters["scene_id"]
        },
        ExpressionAttributeValues: {
          ":s": requestBody.scene,
          ":id_teacher": event.requestContext.authorizer["id-teacher"]
        }
    };
    try {
      await dynamo.update(params).promise();
      statusCode = 204;
      body = {};
    } catch (err) {
      statusCode = 400;
      throw new Error("Erro ao atualizar cena");
    }
  }

  async function delete_scene(event) {
    const params = {
        TableName: table_name,
        Key: {
          id: event.pathParameters["channel_id"]
        },
        ConditionExpression: "id_teacher = :id_teacher",
        UpdateExpression: "remove #s.#n",
        ExpressionAttributeNames: {
          "#s": "scenes",
          "#n": event.pathParameters["scene_id"]
        },
        ExpressionAttributeValues: {
          ":id_teacher": event.requestContext.authorizer["id-teacher"]
        }
    };
    try {
      await dynamo.update(params).promise();
      statusCode = 204;
      body = {};
    } catch (err) {
      statusCode = 400;
      throw new Error("Erro ao deletar cena");
    }
  }

  console.log(event);
  try {
    let requestBody = JSON.parse(event.body);
    let method = event.httpMethod;
    let path = event.resource;
    
    switch (method) {
      case "DELETE":
        if (path == "/channels/{channel_id}") {
          await delete_channel(event);
        } else if (path == "/channels/{channel_id}/scenes/{scene_id}") {
          await delete_scene(event, requestBody);
        }
        break;
        
      case "POST":
        if (path == "/channels/{channel_id}") {
          await get_channel(event, requestBody);
        } else if (path == "/channels") {
          await add_channel(event, requestBody);
        }
        break;
        
      case "GET":
        body = await dynamo
          .scan({ 
            TableName: table_name,
            Key: {
              "id_teacher": event.requestContext.authorizer["id-teacher"]
            },
            FilterExpression: "id_teacher = :id_teacher",
            ExpressionAttributeValues: {
              ":id_teacher": event.requestContext.authorizer["id-teacher"]
            },
            ProjectionExpression:"id, scenes"
          }).promise();
        break;
        
      case "PUT":
        if (path == "/channels/{channel_id}") {
          await update_channel(event, requestBody);
        } else if (path == "/channels/{channel_id}/scenes/{scene_id}") {
          await update_scene(event, requestBody);
        }
        break;
        
      default:
        throw new Error(`Unsupported route: "${event.httpMethod}"`);
    }
  } catch (err) {
    body = {"message": err.message};
  } finally {
    body = JSON.stringify(body);
  }

  return {
    statusCode,
    body,
    headers
  };
};