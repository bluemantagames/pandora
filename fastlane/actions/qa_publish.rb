module Fastlane
  module Actions
    module SharedValues
      QA_PUBLISH_CUSTOM_VALUE = :QA_PUBLISH_CUSTOM_VALUE
    end

    class QaPublishAction < Action
      def self.run(params)
        UI.message "Calling URL: #{params[:webhook_url]}"

        sh "curl -XPOST -H \"Content-Type: application/json\" -d '#{params[:webhook_body]}' '#{params[:webhook_url]}'"
      end

      #####################################################
      # @!group Documentation
      #####################################################

      def self.description
        "Calls a webservice with POST, application/json and provided body"
      end

      def self.available_options
        # Define all options your action supports.

        # Below a few examples
        [
          FastlaneCore::ConfigItem.new(key: :webhook_url,
                                       env_name: "DISCORD_WEBHOOK_URL", # The name of the environment variable
                                       description: "Webhook URL", # a short description of this parameter
                                      ),
          FastlaneCore::ConfigItem.new(key: :webhook_body,
                                       env_name: "DISCORD_WEBHOOK_BODY", # The name of the environment variable
                                       description: "Webhook body", # a short description of this parameter
                                      )
        ]
      end

      def self.output
        # Define the shared values you are going to provide
        # Example
        [
        ]
      end

      def self.return_value
        # If your method provides a return value, you can describe here what it does
      end

      def self.authors
        ["SirStoke"]
      end

      def self.is_supported?(platform)
        true
      end
    end
  end
end
