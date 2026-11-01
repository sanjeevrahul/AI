import { useState } from 'react'
import {
  APIProvider,
  AdvancedMarker,
  Map,
} from '@vis.gl/react-google-maps'

type Location = {
  lat: number
  lng: number
}

const defaultLocation: Location = {
  lat: 53.3498,
  lng: -6.2603,
}

function App() {
  const apiKey =
    import.meta.env.VITE_GOOGLE_MAPS_API_KEY

  const [location, setLocation] =
    useState<Location>(
      defaultLocation,
    )

  const [message, setMessage] =
    useState(
      'Click on the map to select a location.',
    )

  if (!apiKey) {
    return (
      <div className="page">
        <div className="card">
          <h1>Google Maps API Key Missing</h1>

          <p>
            Create a file called:
          </p>

          <pre>.env.local</pre>

          <p>and add:</p>

          <pre>
            VITE_GOOGLE_MAPS_API_KEY=YOUR_API_KEY
          </pre>

          <p>
            Then restart the Vite server.
          </p>
        </div>
      </div>
    )
  }

  function handleMapClick(
    event: google.maps.MapMouseEvent,
  ) {
    if (!event.latLng) {
      return
    }

    const lat =
      event.latLng.lat()

    const lng =
      event.latLng.lng()

    setLocation({
      lat,
      lng,
    })

    setMessage(
      `Selected: ${lat.toFixed(6)}, ${lng.toFixed(6)}`,
    )
  }

  return (
    <APIProvider apiKey={apiKey}>
      <main className="page">

        <h1>
          Google Maps Location Demo
        </h1>

        <p className="subtitle">
          Click anywhere on the map to select
          a location.
        </p>

        <section className="card">

          <div className="map-container">

            <Map
              defaultCenter={
                defaultLocation
              }
              defaultZoom={13}
              mapId="DEMO_MAP_ID"
              onClick={
                handleMapClick
              }
              gestureHandling="greedy"
            >

              <AdvancedMarker
                position={location}
              />

            </Map>

          </div>

        </section>

        <section className="card">

          <h2>
            Selected Location
          </h2>

          <p>
            {message}
          </p>

          <div className="location-info">

            <div>
              <span className="label">
                Latitude
              </span>

              <span className="value">
                {location.lat.toFixed(
                  6,
                )}
              </span>
            </div>

            <div>
              <span className="label">
                Longitude
              </span>

              <span className="value">
                {location.lng.toFixed(
                  6,
                )}
              </span>
            </div>

          </div>

        </section>

      </main>
    </APIProvider>
  )
}

export default App