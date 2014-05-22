﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionConverter.cs" company="Joerg Battermann">
//   Copyright © Joerg Battermann 2014
// </copyright>
// <summary>
//   Defines the PolygonConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GeoJSON.Net.Converters
{
    using System;
    using System.Collections.Generic;

    using GeoJSON.Net.Exceptions;
    using GeoJSON.Net.Geometry;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Text;
    using System.IO;

    /// <summary>
    /// Converter to read and write the <see cref="GeographicPosition" /> type.
    /// </summary>
    public class PositionConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
			  var coordinateElements = value as List<IPosition>;
			  if (coordinateElements != null)
			  {
				  if (coordinateElements.Count == 1) // Point
				  {
					  var coordinate = coordinateElements[0] as GeographicPosition;
					  if (coordinate == null)
					  {
						  serializer.Serialize(writer, null);
						  return;
					  }
					  var coordinateArray = new JArray(coordinate.Longitude, coordinate.Latitude);
					  if (coordinate.Altitude.HasValue && coordinate.Altitude != 0)
						  coordinateArray = new JArray(coordinate.Longitude, coordinate.Latitude, coordinate.Altitude);

					  serializer.Serialize(writer, coordinateArray);
				  }
				  else 
				  {
					  var coordinateArray = new JArray();
					  foreach (var coordinate in coordinateElements)
					  {
						  var position = coordinate as GeographicPosition;
						  if (position == null)
							  continue;

						  JArray positionArray;
						  if (position.Altitude.HasValue && position.Altitude != 0)
							  positionArray = new JArray(position.Longitude, position.Latitude, position.Altitude);
						  else
							  positionArray = new JArray(position.Longitude, position.Latitude);

						  coordinateArray.Add(positionArray);
					  }
					  serializer.Serialize(writer, coordinateArray);
				  }
			  }
			  else
				  serializer.Serialize(writer, value);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var coordinates = serializer.Deserialize<JArray>(reader);
            if (coordinates == null || coordinates.Count != 2)
            {
                throw new ParsingException(
                    string.Format(
                        "Point geometry coordinates could not be parsed. Expected something like '[-122.428938,37.766713]' ([lon,lat]), what we received however was: {0}", 
                        coordinates));
            }

            string latitude;
            string longitude;
            try
            {
                longitude = coordinates.First.ToString();
                latitude = coordinates.Last.ToString();
            }
            catch (Exception ex)
            {
                throw new ParsingException("Could not parse GeoJSON Response. (Latitude or Longitude missing from Point geometry?)", ex);
            }

            return new List<IPosition> { new GeographicPosition(latitude, longitude) };
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GeographicPosition);
        }
    }
}
