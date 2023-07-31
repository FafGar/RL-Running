using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet
{
    struct neuron
    {
        public double [] weights;
        public double bias;
    }


    neuron [] layer1Neurons;
    neuron [] layer2Neurons;
    neuron [] layer3Neurons;
    neuron [] actionNeurons;

    double[] layer1Vals;
    double[] layer2Vals;
    double[] layer3Vals;
    double[] actionVals;

    int numInputs;
    int numOutputOptions;
    int layerSize;
    int maxReward;
    double trainingCoefficient;

    public void init(int numInputs, int outputOptionsCount, int layerSize, int maxReward, double trainingCoefficient){
        this.numInputs = numInputs;
        this.numOutputOptions = outputOptionsCount+1; //adding room for a do nothing option
        this.layerSize = layerSize;
        this.maxReward = maxReward;
        this.trainingCoefficient = trainingCoefficient;

        layer1Neurons = new neuron[layerSize];
        layer2Neurons = new neuron[layerSize];
        layer3Neurons = new neuron[layerSize];
        actionNeurons = new neuron[numOutputOptions];

        //Note for tryin to understand this later: Each layer has a number of neurons. Each neuron has their own set of weights with a # relative to the previous layer's # of neurons.
        for(int i=0;i<layerSize;i++){
            // layer1Neurons[i] = new neuron();
            // layer2Neurons[i] = new neuron();
            // layer3Neurons[i] = new neuron();
            

            layer1Neurons[i].bias = Random.Range(-50,50)/10d;
            layer1Neurons[i].weights = new double[numInputs];

            layer2Neurons[i].bias = Random.Range(-50,50)/10d;
            layer2Neurons[i].weights = new double[layerSize];

            layer3Neurons[i].bias = Random.Range(-50,50)/10d;
            layer3Neurons[i].weights = new double[layerSize];

            for(int j=0; j<numInputs; j++) layer1Neurons[i].weights[j] = Random.Range(1,50)/10d;
            for(int j = 0;j<layerSize;j++){
                layer2Neurons[i].weights[j] = Random.Range(1,50)/10d;
                layer3Neurons[i].weights[j] = Random.Range(1,50)/10d;
            }
            
        }

        for(int i=0;i<numOutputOptions;i++){
            actionNeurons[i] = new neuron();
            actionNeurons[i].bias = Random.Range(1,50)/10d;
            actionNeurons[i].weights = new double[layerSize];

            for(int j = 0;j<layerSize;j++){
                actionNeurons[i].weights[j] = Random.Range(1,50)/10d;
            }
        }
    }

    public int evaluate(float[] inputs){
        if(inputs.Length != this.numInputs){
            return -1;
        }
        layer1Vals = new double[this.layerSize];
        layer2Vals = new double[this.layerSize];
        layer3Vals = new double[this.layerSize];
        actionVals = new double[this.numOutputOptions];

        //Input Eval
        for(int i = 0;i<this.layerSize;i++){
            layer1Vals[i] = this.layer1Neurons[i].bias;
            for(int j = 0; j<this.numInputs; j++) layer1Vals[i] += inputs[j]*this.layer1Neurons[i].weights[j] ;
        }

        //Hidden Layer 1 eval
        for(int i = 0;i<this.layerSize;i++){
            layer2Vals[i] = this.layer1Neurons[i].bias;
            for(int j = 0; j<this.layerSize; j++) layer2Vals[i] += layer1Vals[j]*this.layer2Neurons[i].weights[j] ;
        }

        //Hidden Layer 2 eval
        for(int i = 0;i<this.layerSize;i++){
            layer3Vals[i] = this.layer3Neurons[i].bias;
            for(int j = 0; j<this.layerSize; j++) layer3Vals[i] += layer2Vals[j]*this.layer3Neurons[i].weights[j] ;
        }

        //Output Layer eval
        int max_index = 0;
        for(int i = 0;i<this.numOutputOptions;i++){
            actionVals[i] = this.actionNeurons[i].bias;
            for(int j = 0; j<this.layerSize; j++) actionVals[i] += layer3Vals[j]*this.actionNeurons[i].weights[j];
            if(actionVals[i] >= actionVals[max_index]) max_index = i;
        }
        // Debug.Log(action[0] + " " +action[1] + " " +action[2] + " " );

        // Go through the rest of the layers, applying weights to input from previous layer
        // return index of max value from action option layer
        return max_index;
    }

    public void train(int reward, float [] inputs){
        //strongest and weakest 20% of the final weights are adjusted by adding or
        //  subtracting some multiple of the reward
        //Recurse this process back through the layers until reaching the input layer.


        /* MATH TIME!
            All the weight functions are linear in a multivariable space. So their gradients are always just one value per variable.
            The wieght functinos must be differentiated with respect to the weight, however, so the values are really just the input values.
            Here is the generic weight approximation function:

            w^ = w^ + trainingCoefficient(reward + discountRate(maximum reward value for all neurons added together in current state) - (actual nuerons added up)*gradient)
            
            This evaluation should move the various weights up or down relative to the reward because it and the weights are capable of being negative
            The same thing will be applied to the bias.
        */

        //layer1 training
        for(int i = 0;i<this.layerSize;i++){
            for(int j = 0; j<this.numInputs; j++) this.layer1Neurons[i].weights[j] = this.layer1Neurons[i].weights[j]  + trainingCoefficient(re);
            this.layer1Neurons[i].bias = this.layer1Neurons[i].bias * (1+reward*trainingCoefficient);
        }

        //layer 2 traning
        for(int i = 0;i<this.layerSize;i++){
            for(int j = 0; j<this.layerSize; j++) this.layer2Neurons[i].weights[j] = this.layer2Neurons[i].weights[j] * (1+reward*trainingCoefficient);
            this.layer2Neurons[i].bias = this.layer2Neurons[i].bias * (1+reward*trainingCoefficient);
        }

        //layer 3 training
        for(int i = 0;i<this.layerSize;i++){
            for(int j = 0; j<this.layerSize; j++) this.layer3Neurons[i].weights[j] = this.layer3Neurons[i].weights[j] * (1+reward*trainingCoefficient);
            this.layer3Neurons[i].bias = this.layer3Neurons[i].bias * (1+reward*trainingCoefficient);
        }

        //Output Layer training
        for(int i = 0;i<this.numOutputOptions;i++){
            for(int j = 0; j<this.layerSize; j++) this.actionNeurons[i].weights[j] = this.actionNeurons[i].weights[j] * (1+reward*trainingCoefficient);
            this.actionNeurons[i].bias = this.actionNeurons[i].bias * (1+reward*trainingCoefficient);
        }
    }
}
