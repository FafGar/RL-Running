using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    int layer1MaxI;
    int layer2MaxI;
    int layer3MaxI;
    int outputLayerMaxI;

    double [] oldInputs;
    int numInputs;
    int numOutputOptions;
    int layerSize;
    int maxReward;
    double trainingCoefficient;
    double discountRate;

    /* TODO : Make the layer evaluation go through each possible action. OR MAYBE DON'T? You may not need to because it is already being estimated. I'M TIRED 
    */ 

    public void init(int numInputs, int outputOptionsCount, int layerSize, double discountRate, double trainingCoefficient){
        this.numInputs = numInputs;
        this.numOutputOptions = outputOptionsCount+1; //adding room for a do nothing option
        this.layerSize = layerSize;
        this.trainingCoefficient = trainingCoefficient;
        this.discountRate = discountRate;

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

    public int evaluate(double[] inputs){
    //Note: inputs[0] must be the integer number of the action taken

        if(inputs.Length != this.numInputs){
            return -1;
        }
        this.oldInputs = inputs;

        layer1Vals = new double[this.layerSize];
        layer2Vals = new double[this.layerSize];
        layer3Vals = new double[this.layerSize];
        actionVals = new double[this.numOutputOptions];

        //Input Eval
        // for(int i = 0;i<this.layerSize;i++){
        //     layer1Vals[i] = this.layer1Neurons[i].bias;
        //     for(int j = 0; j<this.numInputs; j++) layer1Vals[i] += inputs[j]*this.layer1Neurons[i].weights[j] ;
        // }
        this.layer1MaxI = this.evalLayer(ref layer1Vals, layer1Neurons, inputs);

        //Hidden Layer 1 eval
        // for(int i = 0;i<this.layerSize;i++){
        //     layer2Vals[i] = this.layer1Neurons[i].bias;
        //     for(int j = 0; j<this.layerSize; j++) layer2Vals[i] += layer1Vals[j]*this.layer2Neurons[i].weights[j] ;
        // }
        this.layer2MaxI = this.evalLayer(ref layer2Vals, layer2Neurons, layer1Vals);

        //Hidden Layer 2 eval
        // for(int i = 0;i<this.layerSize;i++){
        //     layer3Vals[i] = this.layer3Neurons[i].bias;
        //     for(int j = 0; j<this.layerSize; j++) layer3Vals[i] += layer2Vals[j]*this.layer3Neurons[i].weights[j] ;
        // }
        this.layer3MaxI = this.evalLayer(ref layer3Vals, layer3Neurons, layer2Vals);

        //Output Layer eval
        // int max_index = 0;
        // for(int i = 0;i<this.numOutputOptions;i++){
        //     actionVals[i] = this.actionNeurons[i].bias;
        //     for(int j = 0; j<this.layerSize; j++) actionVals[i] += layer3Vals[j]*this.actionNeurons[i].weights[j];
        //     if(actionVals[i] >= actionVals[max_index]) max_index = i;
        // }

        
        // Debug.Log(action[0] + " " +action[1] + " " +action[2] + " " );

        // Go through the rest of the layers, applying weights to input from previous layer
        // return index of max value from action option layer
        this.outputLayerMaxI = this.evalLayer(ref actionVals, actionNeurons, layer3Vals);
        return this.outputLayerMaxI;
    }

    public double getQ(int actionIndex){
        return actionVals[actionIndex];
    }

    public void train(int reward, double [] inputs){
        //Note: inputs[0] must be the integer number of the action taken


        /* MATH TIME!
            All the weight functions are linear in a multivariable space. So their gradients are always just one value per variable.
            The wieght functinos must be differentiated with respect to the weight, however, so the values are really just the input values from the previous layer.
            Here is the generic weight approximation function:

            w^ = w^ + trainingCoefficient(reward + discountRate(maximum predicted reward value in new state) - (Q from this state)*gradient)
            
            This evaluation should move the various weights up or down relative to the reward because it and the weights are capable of being negative
            The same thing will be applied to the bias.
        */

        //Pre-training data
        double actionQ = actionVals[outputLayerMaxI];
        double layer3Q = layer3Vals[layer3MaxI];
        double layer2Q = layer2Vals[layer2MaxI];
        double layer1Q = layer1Vals[layer1MaxI];

        //get new Q values
        this.evaluate(inputs);

        //layer1 training
        double newlayer1Q = layer1Vals[layer1MaxI];
        trainLayer(layer1Neurons, layer1Q, newlayer1Q, reward);
        // for(int i = 0;i<this.layerSize;i++){
        //     for(int j = 0; j<this.numInputs; j++) this.layer1Neurons[i].weights[j] = this.layer1Neurons[i].weights[j]  + trainingCoefficient(re);
        //     this.layer1Neurons[i].bias = this.layer1Neurons[i].bias * (1+reward*trainingCoefficient);
        // }

        //layer 2 traning
        double newlayer2Q = layer2Vals[layer2MaxI];
        trainLayer(layer2Neurons, layer2Q, newlayer2Q, reward);
        // for(int i = 0;i<this.layerSize;i++){
        //     for(int j = 0; j<this.layerSize; j++) this.layer2Neurons[i].weights[j] = this.layer2Neurons[i].weights[j] * (1+reward*trainingCoefficient);
        //     this.layer2Neurons[i].bias = this.layer2Neurons[i].bias * (1+reward*trainingCoefficient);
        // }

        //layer 3 training
        double newlayer3Q = layer3Vals[layer3MaxI];
        trainLayer(layer3Neurons, layer3Q, newlayer3Q, reward);
        // for(int i = 0;i<this.layerSize;i++){
        //     for(int j = 0; j<this.layerSize; j++) this.layer3Neurons[i].weights[j] = this.layer3Neurons[i].weights[j] * (1+reward*trainingCoefficient);
        //     this.layer3Neurons[i].bias = this.layer3Neurons[i].bias * (1+reward*trainingCoefficient);
        // }

        //Output Layer training
        double newActionQ = actionVals[outputLayerMaxI];
        trainLayer(actionNeurons, actionQ, actionQ, reward);
        // double newActionQ = actionVals[outputLayerMaxI];
        // for(int i = 0;i<this.numOutputOptions;i++){
        //     for(int j = 0; j<this.layerSize; j++) this.actionNeurons[i].weights[j] = this.actionNeurons[i].weights[j] + 
        //                                                                                     this.trainingCoefficient*(reward + this.discountRate*());
        //     this.actionNeurons[i].bias = this.actionNeurons[i].bias * (1+reward*trainingCoefficient);
        // }
        }
        
    int evalLayer(ref double[] outputArr, neuron[] neuronArr, double[] inputArr){
        int max_index = 0;
        int length = outputArr.Length;
        for(int i = 0; i< outputArr.Length; i++){
            outputArr[i] = neuronArr[i].bias;
            for(int j = 0; j<inputArr.Length; j++) outputArr[i] += outputArr[j]*neuronArr[i].weights[j];
            if(outputArr[i]>outputArr[max_index]) max_index = i;
        }
        return max_index;
    }

    void trainLayer(neuron[] layerNeruons, double oldQ, double newQ, int reward){
        for(int i = 0;i<layerNeruons.Length;i++){
            for(int j = 0; j<this.oldInputs.Length; j++) this.layer1Neurons[i].weights[j] = this.layer1Neurons[i].weights[j]  + this.trainingCoefficient*(reward + this.discountRate*newQ - oldQ*this.oldInputs[i]);


            // this.layer1Neurons[i].bias = this.layer1Neurons[i].bias * (1+reward*trainingCoefficient);
        }
    }
}
